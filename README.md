# CreditIQ - Credit Default Prediction

CreditIQ is a credit default / credit risk prediction platform built with a .NET Core Web API backend, a React Vite frontend, and a Python ML script integration. It keeps the existing login, register, dashboard, client profile, prediction, ML prediction, and prediction history flow while adding the Lab Course 2 technical foundation.

## Backend Setup

```powershell
cd backend\CreditDefault.Api
dotnet restore
dotnet build
```

Create local configuration through environment variables or an untracked local settings file such as `appsettings.Development.local.json`.

Required environment variables:

```powershell
$env:DB_CONNECTION_STRING=""
$env:JWT_SECRET=""
$env:JWT_ISSUER="CreditDefaultApi"
$env:JWT_AUDIENCE="CreditDefaultFrontend"
$env:PYTHON_EXECUTABLE_PATH="python"
$env:ML_SCRIPT_PATH="ml\predict_credit_risk.py"
```

Use `.env.example` as the safe placeholder reference. Do not commit real secrets.

## Database Migrations

Create migrations from the repository root:

```powershell
dotnet ef migrations add AddLabCourseTechnicalFoundation --project backend\CreditDefault.Api\CreditDefault.Api.csproj
```

Apply migrations:

```powershell
dotnet ef database update --project backend\CreditDefault.Api\CreditDefault.Api.csproj
```

The API also calls `Database.Migrate()` on startup after configuration is provided.

## Run Backend

```powershell
dotnet run --project backend\CreditDefault.Api\CreditDefault.Api.csproj
```

The frontend calls `http://localhost:5086/api` by default.

## Frontend Setup

```powershell
cd frontend
npm install
npm run dev
```

Build the frontend:

```powershell
npm run build
```

## ML Setup

```powershell
cd ml
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

The backend sends prediction payloads to `ml/predict_credit_risk.py`, which loads:

- `ml/models/credit_risk_model.pkl`
- `ml/models/scaler.pkl`

Set `PYTHON_EXECUTABLE_PATH` when the API should use a specific virtual environment Python executable. Set `ML_SCRIPT_PATH` when the script is outside the default repository path.

## Auth And Roles

The Lab Course 2 foundation seeds these roles:

- `Admin`
- `User`
- `Manager`

It also seeds permissions such as `users.read`, `users.manage`, `predictions.create`, `predictions.read`, `predictions.manage`, `reports.read`, `reports.generate`, `settings.manage`, `notifications.read`, and `files.manage`.

JWTs include role claims and permission claims. Existing `[Authorize(Roles = "Admin")]` authorization remains supported.

## Prediction Flow

The Client Analysis page posts applicant data to:

```http
POST /api/predictions/predict
```

The backend validates the request, sends the JSON payload to the Python ML script, stores the prediction, and returns:

```json
{
  "riskLevel": "Medium",
  "riskScore": 62,
  "prediction": 0,
  "explanation": "Applicant has medium credit risk based on loan amount, credit score and debt-to-income ratio."
}
```

## Lab Course 2 Foundation

This step adds normalized roles/permissions, hashed refresh tokens, audit logging for mutating requests, notification/settings/file foundations, and expanded credit-risk domain tables so the database reaches the required relational table count without replacing the existing app architecture.
