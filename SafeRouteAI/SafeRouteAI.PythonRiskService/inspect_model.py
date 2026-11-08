import joblib

bundle = joblib.load("saferoute_s5e10_model.joblib")

print("BUNDLE CONTENTS")
print("================")

for key, value in bundle.items():
    print(f"\nKEY: {key}")
    print("TYPE:", type(value))

    if hasattr(value, "predict"):
        print("PREDICTION MODEL FOUND")
        print("HAS predict_proba:", hasattr(value, "predict_proba"))

    elif isinstance(value, dict):
        print("NESTED KEYS:", list(value.keys()))

        for subkey, subvalue in value.items():
            print(" ", subkey, ":", type(subvalue))
            if hasattr(subvalue, "predict"):
                print("  PREDICTION MODEL FOUND HERE")
