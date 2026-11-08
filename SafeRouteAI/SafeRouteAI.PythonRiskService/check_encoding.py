import joblib

b = joblib.load("saferoute_s5e10_model.joblib")
m = b["model"]

print("OBJECTIVE:", m.params.get("objective"))
print("PANDAS CATEGORICAL METADATA:")
print(m.pandas_categorical)
print("BUNDLE CATEGORIES:")
print(b["categories"])
