"""Train/retrain the SafeRoute AI LightGBM bundle.

Usage: python train_model.py path/to/train.csv
The source CSV is not included in the supplied ZIP. Do not run until its provenance,
units and schema have been checked. This script never silently imputes missing targets.
"""
import sys, json, joblib
import numpy as np
import pandas as pd
import lightgbm as lgb
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error, mean_absolute_error

SRC = sys.argv[1] if len(sys.argv) > 1 else "train.csv"
CATS = {"road_type": ["highway", "rural", "urban"], "lighting": ["daylight", "dim", "night"],
        "weather": ["clear", "foggy", "rainy"], "time_of_day": ["afternoon", "evening", "morning"]}
FEATURES = ["road_type", "num_lanes", "curvature", "speed_limit", "lighting", "weather", "road_signs_present",
            "public_road", "time_of_day", "holiday", "school_season", "num_reported_accidents"]
BOOLS = ["road_signs_present", "public_road", "holiday", "school_season"]
LEVELS = ["Low", "Medium", "High"]
PARAMS = dict(objective="regression", learning_rate=0.05, num_leaves=63, min_child_samples=40,
              feature_fraction=0.9, bagging_fraction=0.8, bagging_freq=1, lambda_l2=1.0,
              verbosity=-1, n_jobs=-1, seed=42)


def prep(df):
    missing = [c for c in FEATURES + ["accident_risk"] if c not in df.columns]
    if missing:
        raise ValueError(f"Dataset is missing required columns: {missing}")
    X = df[FEATURES].copy()
    for b in BOOLS:
        X[b] = pd.to_numeric(X[b], errors="coerce")
    for c in ("num_lanes", "curvature", "speed_limit", "num_reported_accidents"):
        X[c] = pd.to_numeric(X[c], errors="coerce")
    for c, cats in CATS.items():
        X[c] = pd.Categorical(X[c].astype("string").str.lower().str.strip(), categories=cats)
    return X


df = pd.read_csv(SRC)
# Remove exact duplicate feature/target rows before splitting; exclude arbitrary ID columns.
df = df.drop_duplicates(subset=FEATURES + ["accident_risk"]).copy()
X_all = prep(df)
y_all = pd.to_numeric(df["accident_risk"], errors="coerce")
valid = y_all.notna() & X_all.notna().all(axis=1)
for b in BOOLS:
    valid &= X_all[b].isin([0, 1])
removed = int((~valid).sum())
X_all, y_all = X_all.loc[valid], y_all.loc[valid]
if len(X_all) < 100:
    raise ValueError("Too few complete rows to train safely")
if not y_all.between(0, 1).all():
    raise ValueError("accident_risk must be between 0 and 1")

# Split BEFORE computing risk-band thresholds: avoids validation target leakage.
Xtr, Xva, ytr, yva = train_test_split(X_all, y_all, test_size=0.2, random_state=42)
q_train = np.quantile(ytr, [1 / 3, 2 / 3])
train_set = lgb.Dataset(Xtr, ytr, categorical_feature=list(CATS))
valid_set = lgb.Dataset(Xva, yva, categorical_feature=list(CATS), reference=train_set)
model = lgb.train(PARAMS, train_set, 3000, valid_sets=[valid_set],
                  callbacks=[lgb.early_stopping(100, verbose=False), lgb.log_evaluation(0)])
pred = np.clip(model.predict(Xva, num_iteration=model.best_iteration), 0, 1)

def bands(values):
    return np.digitize(values, q_train)

dist = np.min(np.abs(pred[:, None] - q_train[None, :]), axis=1)
confident = dist >= 0.04
all_acc = float((bands(pred) == bands(yva.to_numpy())).mean())
conf_acc = float((bands(pred)[confident] == bands(yva.to_numpy())[confident]).mean()) if confident.any() else None
metrics = {"rmse": float(mean_squared_error(yva, pred) ** 0.5),
           "mae": float(mean_absolute_error(yva, pred)),
           "band_accuracy_all": all_acc,
           "confident_share_margin_0.04": float(confident.mean()),
           "band_accuracy_confident": conf_acc,
           "cutoffs_from_training_only": [float(v) for v in q_train],
           "iterations": int(model.best_iteration), "rows_used": int(len(X_all)),
           "rows_removed_missing_or_invalid": removed,
           "validation_method": "random holdout; dataset has no verified geographic/time grouping in this script",
           "speed_limit_units": "UNVERIFIED - inspect original dataset documentation before interpreting units"}

# Optional XGBoost comparison on exactly the same split. It is not required to run.
try:
    from xgboost import XGBRegressor
    Xtr_x, Xva_x = Xtr.copy(), Xva.copy()
    for c in CATS:
        Xtr_x[c] = Xtr_x[c].cat.codes
        Xva_x[c] = Xva_x[c].cat.codes
    xgb_model = XGBRegressor(n_estimators=1000, max_depth=6, learning_rate=0.04,
                             subsample=0.8, colsample_bytree=0.9, objective="reg:squarederror",
                             n_jobs=-1, random_state=42, early_stopping_rounds=80)
    xgb_model.fit(Xtr_x, ytr, eval_set=[(Xva_x, yva)], verbose=False)
    xp = np.clip(xgb_model.predict(Xva_x), 0, 1)
    metrics["xgboost_comparison"] = {"rmse": float(mean_squared_error(yva, xp) ** 0.5),
                                     "mae": float(mean_absolute_error(yva, xp)),
                                     "band_accuracy_same_training_cutoffs": float((bands(xp) == bands(yva.to_numpy())).mean())}
except ImportError:
    metrics["xgboost_comparison"] = "Not run: install xgboost to benchmark."

# Refit final LightGBM model on all available training data, using the selected iteration count.
final = lgb.train(PARAMS, lgb.Dataset(X_all, y_all, categorical_feature=list(CATS)),
                  max(1, int(model.best_iteration)))
full_q = np.quantile(y_all, [1 / 3, 2 / 3])
joblib.dump({"model": final, "features": FEATURES, "categories": CATS,
             "thresholds": {"low_max": round(float(full_q[0]), 3), "medium_max": round(float(full_q[1]), 3)},
             "levels": LEVELS, "metadata": {"model": "LightGBM", "speed_limit_units": "unverified; inspect source data",
                                             "training_rows": int(len(X_all))}},
            "saferoute_s5e10_model.joblib")
with open("metrics.json", "w", encoding="utf-8") as f:
    json.dump(metrics, f, indent=2)
print(json.dumps(metrics, indent=2))
