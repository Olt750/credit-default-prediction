# CreditIQ

## Running The App

### Backend

```powershell
dotnet run --project backend\CreditDefault.Api\CreditDefault.Api.csproj
```

The API runs from the ASP.NET launch profile. The frontend is configured to call `http://localhost:5086/api`.

### Frontend

```powershell
cd frontend
npm install
npm run dev
```

Open the Vite URL, sign in, and go to `/clients`.

### Python ML Requirements

```powershell
cd ml
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

If your backend should use a specific Python executable, add `ML:PythonExecutable` to backend configuration. By default, the API calls `python`.

## Prediction Flow

The Client Analysis page posts applicant data to:

```http
POST /api/predictions/predict
```

The backend validates the request, sends the JSON payload to `ml/predict_credit_risk.py`, and the Python script loads:

- `ml/models/credit_risk_model.pkl`
- `ml/models/scaler.pkl`

The Python layer maps the frontend fields onto the 20 German Credit numeric model features:

`laufkont, laufzeit, moral, verw, hoehe, sparkont, beszeit, rate, famges, buerge, wohnzeit, verm, alter, weitkred, wohn, bishkred, beruf, pers, telef, gastarb`

The trained German Credit fields are not a one-to-one match for the UI form, so the mapping layer documents the approximations in code. Direct mappings include `age -> alter`, `loanAmount -> hoehe`, and `loanTerm -> laufzeit`. Other UI fields such as credit score, employment status, education, marital status, previous defaults, and debt-to-income ratio are converted into reasonable encoded German Credit values.

The final response returns to the frontend as:

```json
{
  "riskLevel": "Medium",
  "riskScore": 62,
  "prediction": 0,
  "explanation": "Applicant has medium credit risk based on loan amount, credit score and debt-to-income ratio."
}
```

The page then displays the risk level, score, prediction status, and explanation with green, yellow/orange, or red styling.
