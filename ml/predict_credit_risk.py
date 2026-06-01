import json
import sys
from pathlib import Path

import joblib
import numpy as np


FEATURE_ORDER = [
    "laufkont",
    "laufzeit",
    "moral",
    "verw",
    "hoehe",
    "sparkont",
    "beszeit",
    "rate",
    "famges",
    "buerge",
    "wohnzeit",
    "verm",
    "alter",
    "weitkred",
    "wohn",
    "bishkred",
    "beruf",
    "pers",
    "telef",
    "gastarb",
]


BASE_DIR = Path(__file__).resolve().parent
MODEL_PATH = BASE_DIR / "models" / "credit_risk_model.pkl"
SCALER_PATH = BASE_DIR / "models" / "scaler.pkl"


def clamp(value, minimum, maximum):
    return max(minimum, min(maximum, value))


def map_to_german_credit_features(payload):
    age = int(payload["age"])
    annual_income = float(payload["annualIncome"])
    loan_amount = float(payload["loanAmount"])
    credit_score = int(payload["creditScore"])
    loan_term = int(payload["loanTerm"])
    previous_defaults = int(payload.get("previousDefaults", 0))
    debt_to_income = float(payload["debtToIncomeRatio"])
    employment_status = str(payload.get("employmentStatus", "")).strip().lower()
    education = str(payload.get("education", "")).strip().lower()
    marital_status = str(payload.get("maritalStatus", "")).strip().lower()

    # Mapping notes:
    # - age, loan amount, and loan term map directly to alter, hoehe, and laufzeit.
    # - The frontend has no German Credit checking/savings account fields, so income,
    #   credit score, and employment status are used as practical approximations.
    # - debtToIncomeRatio is converted to the German Credit installment-rate scale (1-4).
    # - previousDefaults influences moral (credit history) and bishkred (existing credits).
    laufkont = 4 if credit_score >= 720 else 3 if credit_score >= 660 else 2 if credit_score >= 580 else 1
    moral = 4 if previous_defaults == 0 and credit_score >= 680 else 2 if previous_defaults <= 1 else 1
    verw = 2 if loan_amount <= 2500 else 3 if loan_amount <= 8000 else 9
    sparkont = 5 if annual_income >= 80000 else 4 if annual_income >= 50000 else 3 if annual_income >= 30000 else 1
    beszeit = {
        "employed": 4,
        "self-employed": 3,
        "retired": 3,
        "student": 2,
        "unemployed": 1,
    }.get(employment_status, 2)
    rate = 1 if debt_to_income < 0.2 else 2 if debt_to_income < 0.35 else 3 if debt_to_income < 0.5 else 4
    famges = {
        "single": 2,
        "married": 3,
        "divorced": 1,
        "widowed": 4,
    }.get(marital_status, 2)
    wohnzeit = 4 if age >= 35 else 2
    verm = 1 if annual_income >= 60000 else 2 if annual_income >= 35000 else 3
    wohn = 2 if annual_income >= 30000 else 1
    bishkred = clamp(previous_defaults + 1, 1, 4)
    beruf = 4 if education in {"master", "phd"} else 3 if education == "bachelor" else 2
    pers = 2 if annual_income >= 25000 else 1
    telef = 2 if annual_income >= 30000 else 1
    gastarb = 2

    values = {
        "laufkont": laufkont,
        "laufzeit": loan_term,
        "moral": moral,
        "verw": verw,
        "hoehe": loan_amount,
        "sparkont": sparkont,
        "beszeit": beszeit,
        "rate": rate,
        "famges": famges,
        "buerge": 1,
        "wohnzeit": wohnzeit,
        "verm": verm,
        "alter": age,
        "weitkred": 3,
        "wohn": wohn,
        "bishkred": bishkred,
        "beruf": beruf,
        "pers": pers,
        "telef": telef,
        "gastarb": gastarb,
    }

    return np.array([[values[name] for name in FEATURE_ORDER]], dtype=float)


def risk_level(score):
    if score < 35:
        return "Low"
    if score < 70:
        return "Medium"
    return "High"


def build_explanation(level, score, payload):
    parts = []
    if int(payload["creditScore"]) < 650:
        parts.append("credit score")
    if float(payload["debtToIncomeRatio"]) >= 0.35:
        parts.append("debt-to-income ratio")
    if float(payload["loanAmount"]) > float(payload["annualIncome"]) * 0.4:
        parts.append("loan amount")
    if int(payload.get("previousDefaults", 0)) > 0:
        parts.append("previous default history")

    if not parts:
        parts = ["income stability", "credit score", "loan term"]

    drivers = ", ".join(parts[:-1]) + (" and " + parts[-1] if len(parts) > 1 else parts[0])
    return f"Applicant has {level.lower()} credit risk based on {drivers}."


def predict(payload):
    model = joblib.load(MODEL_PATH)
    scaler = joblib.load(SCALER_PATH)
    features = map_to_german_credit_features(payload)
    model_name = type(model).__name__.lower()

    # The current saved RandomForestClassifier was trained on raw German Credit
    # features in the notebook. The scaler is still loaded because it is part of
    # the model artifact set, but it should only be applied to scaled estimators.
    model_features = features
    if not any(name in model_name for name in ("forest", "tree")):
        model_features = scaler.transform(features)

    prediction = int(model.predict(model_features)[0])
    if hasattr(model, "predict_proba"):
        classes = list(model.classes_)
        bad_class_index = classes.index(0) if 0 in classes else 0
        bad_probability = float(model.predict_proba(model_features)[0][bad_class_index])
        score = int(round(bad_probability * 100))
    else:
        score = 75 if prediction == 0 else 25

    level = risk_level(score)
    return {
        "riskLevel": level,
        "riskScore": score,
        "prediction": prediction,
        "explanation": build_explanation(level, score, payload),
    }


def main():
    try:
        payload = json.load(sys.stdin)
        print(json.dumps(predict(payload)))
    except Exception as exc:
        print(json.dumps({"error": str(exc)}), file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
