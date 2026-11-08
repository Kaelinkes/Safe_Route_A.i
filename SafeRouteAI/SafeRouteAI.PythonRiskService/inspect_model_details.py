import joblib

bundle = joblib.load("saferoute_s5e10_model.joblib")
model = bundle["model"]

print("\nACTUAL MODEL FEATURES")
print("=====================")
for i, feature in enumerate(bundle["features"], 1):
    print(f"{i}. {feature}")

print("\nCATEGORIES")
print(bundle["categories"])

print("\nRISK THRESHOLDS")
print(bundle["thresholds"])

print("\nRISK LEVELS")
print(bundle["levels"])

print("\nLIGHTGBM DETAILS")
print("Feature count:", model.num_feature())
print("Feature names:", model.feature_name())
print("Best iteration:", model.best_iteration)
