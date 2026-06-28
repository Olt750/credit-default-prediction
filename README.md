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
$env:REDIS_CONNECTION_STRING="localhost:6379"
$env:REDIS_INSTANCE_NAME="CreditDefault"
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

## Redis / NoSQL Setup

Redis is used as the Lab Course 2 NoSQL component for cache-backed application data:

- dashboard summaries: `dashboard:user:{userId}`, `dashboard:admin`
- recent predictions: `predictions:recent:{userId}:{limit}`, `predictions:recent:admin:{limit}`
- notifications: `notifications:unread:{userId}`, `notifications:list:{userId}:page:{page}:size:{pageSize}`

Run Redis locally with Docker:

```powershell
docker run --name creditdefault-redis -p 6379:6379 -d redis:latest
```

Check that Redis is running:

```powershell
docker ps --filter "name=creditdefault-redis"
docker exec -it creditdefault-redis redis-cli ping
```

Set `REDIS_CONNECTION_STRING` to your Redis endpoint. If Redis is offline or unavailable, the API logs cache warnings and continues serving requests from SQL Server.

## Real-Time Notifications

SignalR is exposed at:

```http
/hubs/notifications
```

Authenticated clients join `user:{userId}` and role groups such as `role:Admin`. Events include:

- `NotificationReceived`
- `UnreadCountUpdated`
- `PredictionCompleted`
- `HighRiskAlert`
- `SettingsChanged`
- `ReportGenerated`
- `ImportCompleted`

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

## Dynamic Reports

Reports are generated from real SQL prediction, user, model metric, and report data. The Reports page is available at:

```http
/reports
```

Supported report types:

- Prediction Summary Report
- Risk Distribution Report
- User Prediction History Report
- High Risk Applications Report
- Model Performance Report

Supported export formats:

- CSV
- Excel-compatible XLSX
- JSON

PDF endpoints are intentionally not implemented until a proper PDF renderer is added.

Main report endpoints:

```http
GET /api/reports
POST /api/reports/generate
GET /api/reports/{id}
GET /api/reports/{id}/download
DELETE /api/reports/{id}
```

Reports store metadata in SQL Server and generated files under `backend/CreditDefault.Api/exports/reports`, which is ignored by Git. Redis caches report summaries when available. SignalR sends `ReportGenerated`, and high-risk admin notifications are emitted when appropriate.

## Export And Import

The app supports real export/import for these lists:

- Users
- Predictions
- Client Profiles
- Notifications
- Reports

Admin-only:

- Users import/export
- Audit logs export
- System-wide exports

Export endpoints:

```http
GET /api/export/users?format=csv|xlsx|json
GET /api/export/predictions?format=csv|xlsx|json
GET /api/export/client-profiles?format=csv|xlsx|json
GET /api/export/notifications?format=csv|xlsx|json
GET /api/export/reports?format=csv|xlsx|json
GET /api/export/audit-logs?format=csv|xlsx|json
```

Import endpoints:

```http
POST /api/import/users
POST /api/import/predictions
POST /api/import/client-profiles
POST /api/import/notifications
POST /api/import/reports
```

Imports accept CSV and JSON. Excel import is rejected with a clear message until a safe parser is added. Import responses include total, inserted, skipped, failed, and errors. Password hashes, refresh tokens, JWT secrets, and config secrets are never exported. User imports require a `Password` field, which is hashed before storage.

## Advanced Search

The backend supports keyword search, filters, sorting, and pagination for:

- Users
- Predictions
- Client Profiles
- Notifications
- Reports

Search endpoints:

```http
GET /api/search/users
GET /api/search/predictions
GET /api/search/client-profiles
GET /api/search/notifications
GET /api/search/reports
```

Users can only search/export their own predictions, profile, notifications, and reports. Admin can access all supported lists. Manager can access reports and predictions where authorization allows it. Frontend search/filter controls were added to Users, Predictions, Client Analysis, Notifications, and Reports.

## Machine Learning Experiments And Reporting

```powershell
cd ml
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

Dataset:

- `ml/dataset/german_credit_data.csv`
- 1,000 German Credit applications
- target column: `kredit`
- target mapping: `0` = bad/default-risk credit, `1` = good/non-default credit

Run the full experiment pipeline:

```powershell
cd ml
.\.venv\Scripts\python.exe train_model.py
```

Open the project notebook:

```powershell
cd ml
jupyter notebook notebooks/credit_risk_analysis.ipynb
```

The notebook documents:

- dataset description and target definition
- missing-value and class-distribution checks
- categorical/numerical feature handling
- stratified train/test split
- model training and evaluation
- hyperparameter tuning
- feature selection
- neural network architecture comparison
- KMeans clustering
- final model discussion, limitations, and future work

Implemented classifiers:

- Logistic Regression
- K-Nearest Neighbors
- Decision Tree
- Random Forest
- Neural Network / MLPClassifier
- Support Vector Machine

Each classifier is tuned with GridSearchCV and evaluated with accuracy, precision, recall, F1-score, ROC-AUC, and confusion matrix values on the same stratified test set. Hyperparameter grids are stored in `ml/train_model.py` and the generated comparison files.

Feature selection / reduction:

- SelectKBest with top 5, top 10, and top 15 features
- Random Forest feature importance with top 5, top 10, and top 15 features

Neural network experiments:

- Architecture 1: hidden layers `(64, 32)`, ReLU, Adam, learning rate `0.001`
- Architecture 2: hidden layers `(128, 64, 32)`, ReLU, Adam, learning rate `0.001`
- Architecture 3: hidden layers `(32, 16)`, tanh, Adam, learning rate `0.001`

Clustering experiments:

- KMeans with `k = 2, 3, 4, 5`
- metrics: silhouette score, inertia, Adjusted Rand Index
- PCA scatter data and charts are generated without using the target label during clustering

Generated result files:

- `ml/results/model_comparison_results.csv`
- `ml/results/model_comparison_results.json`
- `ml/results/feature_importance.csv`
- `ml/results/feature_importance.json`
- `ml/results/selected_features.csv`
- `ml/results/feature_selection_results.csv`
- `ml/results/feature_selection_results.json`
- `ml/results/neural_network_results.csv`
- `ml/results/neural_network_results.json`
- `ml/results/clustering_results.csv`
- `ml/results/clustering_results.json`
- `ml/results/clustering_points.json`
- `ml/results/experiment_summary.json`
- chart PNGs for feature importance, neural networks, and KMeans clustering

Generated model artifacts:

- `ml/models/credit_risk_model.pkl`
- `ml/models/best_credit_model.pkl`
- `ml/models/preprocessor.pkl`
- `ml/models/scaler.pkl`
- `ml/models/feature_columns.pkl`
- `ml/models/model_metadata.json`

The backend sends prediction payloads to `ml/predict_credit_risk.py`, which loads the final trained model pipeline and keeps fallback compatibility with the older model/scaler artifacts.

Set `PYTHON_EXECUTABLE_PATH` when the API should use a specific virtual environment Python executable. Set `ML_SCRIPT_PATH` when the script is outside the default repository path.

ML result API endpoints:

```http
GET /api/ml/model-comparison
GET /api/ml/feature-importance
GET /api/ml/feature-selection
GET /api/ml/neural-network-results
GET /api/ml/clustering-results
GET /api/ml/clustering-points
GET /api/ml/model-metadata
GET /api/ml/summary
GET /api/admin/ml/model-comparison
GET /api/admin/ml/summary
```

The Models, Neural Network, and Clustering frontend pages read these endpoints and no longer rely on mock ML metrics.

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
