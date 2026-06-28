import json
import time
from datetime import datetime, timezone
from pathlib import Path

import joblib
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from sklearn.base import clone
from sklearn.cluster import KMeans
from sklearn.decomposition import PCA
from sklearn.ensemble import RandomForestClassifier
from sklearn.feature_selection import SelectKBest, f_classif
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import (
    accuracy_score,
    adjusted_rand_score,
    confusion_matrix,
    f1_score,
    precision_score,
    recall_score,
    roc_auc_score,
    make_scorer,
    silhouette_score,
)
from sklearn.model_selection import GridSearchCV, StratifiedKFold, train_test_split
from sklearn.neighbors import KNeighborsClassifier
from sklearn.neural_network import MLPClassifier
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import StandardScaler
from sklearn.svm import SVC
from sklearn.tree import DecisionTreeClassifier


BASE_DIR = Path(__file__).resolve().parent
DATA_PATH = BASE_DIR / "dataset" / "german_credit_data.csv"
RESULTS_DIR = BASE_DIR / "results"
MODELS_DIR = BASE_DIR / "models"
TARGET = "kredit"
RANDOM_STATE = 42
BAD_CLASS = 0

FEATURE_DESCRIPTIONS = {
    "laufkont": "checking account status",
    "laufzeit": "credit duration in months",
    "moral": "credit history",
    "verw": "loan purpose",
    "hoehe": "credit amount",
    "sparkont": "savings account status",
    "beszeit": "employment duration",
    "rate": "installment rate as income percentage",
    "famges": "personal status and sex",
    "buerge": "other debtors or guarantors",
    "wohnzeit": "years at current residence",
    "verm": "property category",
    "alter": "age in years",
    "weitkred": "other installment plans",
    "wohn": "housing status",
    "bishkred": "number of existing credits",
    "beruf": "job category",
    "pers": "number of liable people",
    "telef": "telephone ownership",
    "gastarb": "foreign worker indicator",
}


def ensure_dirs():
    RESULTS_DIR.mkdir(exist_ok=True)
    MODELS_DIR.mkdir(exist_ok=True)


def json_default(value):
    if isinstance(value, (np.integer,)):
        return int(value)
    if isinstance(value, (np.floating,)):
        return float(value)
    if isinstance(value, np.ndarray):
        return value.tolist()
    return str(value)


def write_json(path, data):
    path.write_text(json.dumps(data, indent=2, default=json_default), encoding="utf-8")


def normalize_params(params):
    normalized = {}
    for key, value in params.items():
        clean_key = key.replace("model__", "")
        normalized[clean_key] = value
    return normalized


def evaluate(model, x_test, y_test):
    predictions = model.predict(x_test)
    if hasattr(model, "predict_proba"):
        scores = model.predict_proba(x_test)[:, list(model.classes_).index(BAD_CLASS)]
    elif hasattr(model, "decision_function"):
        raw_scores = model.decision_function(x_test)
        scores = -raw_scores if BAD_CLASS == 0 else raw_scores
    else:
        scores = predictions == BAD_CLASS

    matrix = confusion_matrix(y_test, predictions, labels=[1, 0])
    tn, fp, fn, tp = matrix.ravel()
    return {
        "accuracy": accuracy_score(y_test, predictions),
        "precision": precision_score(y_test, predictions, pos_label=BAD_CLASS, zero_division=0),
        "recall": recall_score(y_test, predictions, pos_label=BAD_CLASS, zero_division=0),
        "f1": f1_score(y_test, predictions, pos_label=BAD_CLASS, zero_division=0),
        "roc_auc": roc_auc_score((y_test == BAD_CLASS).astype(int), scores),
        "confusion_matrix": {
            "tn": int(tn),
            "fp": int(fp),
            "fn": int(fn),
            "tp": int(tp),
            "labels": ["Good credit", "Bad credit"],
        },
    }


def model_configs():
    return {
        "Logistic Regression": {
            "pipeline": Pipeline([
                ("scaler", StandardScaler()),
                ("model", LogisticRegression(max_iter=1000, class_weight="balanced", random_state=RANDOM_STATE)),
            ]),
            "params": {
                "model__C": [0.01, 0.1, 1.0, 10.0],
                "model__penalty": ["l2"],
                "model__solver": ["lbfgs", "liblinear"],
            },
        },
        "K-Nearest Neighbors": {
            "pipeline": Pipeline([
                ("scaler", StandardScaler()),
                ("model", KNeighborsClassifier()),
            ]),
            "params": {
                "model__n_neighbors": [3, 5, 7, 11],
                "model__weights": ["uniform", "distance"],
                "model__metric": ["euclidean", "manhattan"],
            },
        },
        "Decision Tree": {
            "pipeline": Pipeline([
                ("model", DecisionTreeClassifier(random_state=RANDOM_STATE, class_weight="balanced")),
            ]),
            "params": {
                "model__max_depth": [3, 5, 8, None],
                "model__min_samples_split": [2, 10, 20],
                "model__min_samples_leaf": [1, 5, 10],
                "model__criterion": ["gini", "entropy"],
            },
        },
        "Random Forest": {
            "pipeline": Pipeline([
                ("model", RandomForestClassifier(random_state=RANDOM_STATE, class_weight="balanced", n_jobs=-1)),
            ]),
            "params": {
                "model__n_estimators": [100, 200],
                "model__max_depth": [5, 10, None],
                "model__min_samples_split": [2, 10],
                "model__min_samples_leaf": [1, 5],
                "model__max_features": ["sqrt", "log2"],
            },
        },
        "Neural Network": {
            "pipeline": Pipeline([
                ("scaler", StandardScaler()),
                ("model", MLPClassifier(random_state=RANDOM_STATE, solver="adam", early_stopping=True)),
            ]),
            "params": {
                "model__hidden_layer_sizes": [(64, 32), (128, 64, 32), (32, 16)],
                "model__activation": ["relu", "tanh"],
                "model__alpha": [0.0001, 0.001],
                "model__learning_rate_init": [0.001, 0.01],
                "model__max_iter": [300],
            },
        },
        "Support Vector Machine": {
            "pipeline": Pipeline([
                ("scaler", StandardScaler()),
                ("model", SVC(class_weight="balanced", probability=True, random_state=RANDOM_STATE)),
            ]),
            "params": {
                "model__C": [0.1, 1.0, 10.0],
                "model__kernel": ["rbf", "linear"],
                "model__gamma": ["scale", "auto"],
            },
        },
    }


def train_and_compare(x_train, x_test, y_train, y_test):
    cv = StratifiedKFold(n_splits=5, shuffle=True, random_state=RANDOM_STATE)
    bad_credit_f1 = make_scorer(f1_score, pos_label=BAD_CLASS, zero_division=0)
    rows = []
    trained = {}
    for name, config in model_configs().items():
        baseline = clone(config["pipeline"])
        baseline.fit(x_train, y_train)
        baseline_metrics = evaluate(baseline, x_test, y_test)

        search = GridSearchCV(
            clone(config["pipeline"]),
            config["params"],
            scoring=bad_credit_f1,
            cv=cv,
            n_jobs=-1,
            error_score="raise",
        )
        search.fit(x_train, y_train)
        tuned_metrics = evaluate(search.best_estimator_, x_test, y_test)

        row = {
            "model": name,
            "tested_hyperparameters": json.dumps({k.replace("model__", ""): v for k, v in config["params"].items()}),
            "best_hyperparameters": json.dumps(normalize_params(search.best_params_)),
            "cv_score": search.best_score_,
            "baseline_accuracy": baseline_metrics["accuracy"],
            "baseline_precision": baseline_metrics["precision"],
            "baseline_recall": baseline_metrics["recall"],
            "baseline_f1": baseline_metrics["f1"],
            "test_accuracy": tuned_metrics["accuracy"],
            "test_precision": tuned_metrics["precision"],
            "test_recall": tuned_metrics["recall"],
            "test_f1": tuned_metrics["f1"],
            "roc_auc": tuned_metrics["roc_auc"],
            "confusion_matrix": tuned_metrics["confusion_matrix"],
        }
        rows.append(row)
        trained[name] = search.best_estimator_

    comparison = pd.DataFrame(rows).sort_values("test_f1", ascending=False)
    comparison.to_csv(RESULTS_DIR / "model_comparison_results.csv", index=False)
    write_json(RESULTS_DIR / "model_comparison_results.json", comparison.to_dict(orient="records"))
    return comparison, trained


def feature_selection(x_train, x_test, y_train, y_test, feature_names):
    rf = RandomForestClassifier(n_estimators=300, random_state=RANDOM_STATE, class_weight="balanced", n_jobs=-1)
    rf.fit(x_train, y_train)
    importance = pd.DataFrame({
        "feature": feature_names,
        "description": [FEATURE_DESCRIPTIONS.get(f, f) for f in feature_names],
        "importance": rf.feature_importances_,
    }).sort_values("importance", ascending=False)
    importance.to_csv(RESULTS_DIR / "feature_importance.csv", index=False)
    write_json(RESULTS_DIR / "feature_importance.json", importance.to_dict(orient="records"))

    plt.figure(figsize=(10, 6))
    top = importance.head(15).sort_values("importance")
    plt.barh(top["feature"], top["importance"], color="#2563eb")
    plt.title("Random Forest Feature Importance")
    plt.xlabel("Importance")
    plt.tight_layout()
    plt.savefig(RESULTS_DIR / "feature_importance.png", dpi=160)
    plt.close()

    rows = []
    selected_rows = []
    baseline = Pipeline([
        ("model", RandomForestClassifier(n_estimators=200, random_state=RANDOM_STATE, class_weight="balanced", n_jobs=-1)),
    ])
    baseline.fit(x_train, y_train)
    base_metrics = evaluate(baseline, x_test, y_test)
    rows.append({
        "method": "All Features",
        "k": len(feature_names),
        "selected_features": ",".join(feature_names),
        **{f"test_{k}": v for k, v in base_metrics.items() if k != "confusion_matrix"},
    })

    for k in [5, 10, min(15, len(feature_names))]:
        selector = SelectKBest(score_func=f_classif, k=k)
        select_model = Pipeline([
            ("scaler", StandardScaler()),
            ("selector", selector),
            ("model", LogisticRegression(max_iter=1000, class_weight="balanced", random_state=RANDOM_STATE)),
        ])
        select_model.fit(x_train, y_train)
        metrics = evaluate(select_model, x_test, y_test)
        mask = select_model.named_steps["selector"].get_support()
        features = [feature_names[i] for i, selected in enumerate(mask) if selected]
        rows.append({
            "method": "SelectKBest",
            "k": k,
            "selected_features": ",".join(features),
            **{f"test_{key}": value for key, value in metrics.items() if key != "confusion_matrix"},
        })
        for feature in features:
            selected_rows.append({"method": "SelectKBest", "k": k, "feature": feature})

        rf_features = importance.head(k)["feature"].tolist()
        rf_model = Pipeline([
            ("model", RandomForestClassifier(n_estimators=200, random_state=RANDOM_STATE, class_weight="balanced", n_jobs=-1)),
        ])
        rf_model.fit(x_train[rf_features], y_train)
        rf_metrics = evaluate(rf_model, x_test[rf_features], y_test)
        rows.append({
            "method": "Random Forest Importance",
            "k": k,
            "selected_features": ",".join(rf_features),
            **{f"test_{key}": value for key, value in rf_metrics.items() if key != "confusion_matrix"},
        })
        for feature in rf_features:
            selected_rows.append({"method": "Random Forest Importance", "k": k, "feature": feature})

    pd.DataFrame(selected_rows).to_csv(RESULTS_DIR / "selected_features.csv", index=False)
    results = pd.DataFrame(rows).sort_values("test_f1", ascending=False)
    results.to_csv(RESULTS_DIR / "feature_selection_results.csv", index=False)
    write_json(RESULTS_DIR / "feature_selection_results.json", results.to_dict(orient="records"))
    return importance, results


def neural_networks(x_train, x_test, y_train, y_test):
    configs = [
        {"architecture": "Architecture 1", "hidden_layer_sizes": (64, 32), "activation": "relu", "solver": "adam", "learning_rate_init": 0.001},
        {"architecture": "Architecture 2", "hidden_layer_sizes": (128, 64, 32), "activation": "relu", "solver": "adam", "learning_rate_init": 0.001},
        {"architecture": "Architecture 3", "hidden_layer_sizes": (32, 16), "activation": "tanh", "solver": "adam", "learning_rate_init": 0.001},
    ]
    rows = []
    for config in configs:
        start = time.perf_counter()
        model = Pipeline([
            ("scaler", StandardScaler()),
            ("model", MLPClassifier(
                hidden_layer_sizes=config["hidden_layer_sizes"],
                activation=config["activation"],
                solver=config["solver"],
                learning_rate_init=config["learning_rate_init"],
                max_iter=500,
                alpha=0.0001,
                early_stopping=True,
                random_state=RANDOM_STATE,
            )),
        ])
        model.fit(x_train, y_train)
        elapsed = time.perf_counter() - start
        metrics = evaluate(model, x_test, y_test)
        rows.append({
            **config,
            "layers": [f"Input ({x_train.shape[1]})"] + [f"Dense {n} - {config['activation']}" for n in config["hidden_layer_sizes"]] + ["Output - default risk class"],
            "training_time_seconds": elapsed,
            "accuracy": metrics["accuracy"],
            "precision": metrics["precision"],
            "recall": metrics["recall"],
            "f1": metrics["f1"],
            "roc_auc": metrics["roc_auc"],
            "confusion_matrix": metrics["confusion_matrix"],
        })

    df = pd.DataFrame(rows)
    df.to_csv(RESULTS_DIR / "neural_network_results.csv", index=False)
    write_json(RESULTS_DIR / "neural_network_results.json", df.to_dict(orient="records"))

    plt.figure(figsize=(8, 5))
    x = np.arange(len(df))
    plt.bar(x - 0.2, df["accuracy"], width=0.2, label="Accuracy")
    plt.bar(x, df["recall"], width=0.2, label="Recall")
    plt.bar(x + 0.2, df["f1"], width=0.2, label="F1")
    plt.xticks(x, df["architecture"], rotation=15)
    plt.ylim(0, 1)
    plt.title("Neural Network Architecture Comparison")
    plt.legend()
    plt.tight_layout()
    plt.savefig(RESULTS_DIR / "neural_network_comparison.png", dpi=160)
    plt.close()
    return df


def cluster_experiments(x, y):
    scaler = StandardScaler()
    x_scaled = scaler.fit_transform(x)
    pca = PCA(n_components=2, random_state=RANDOM_STATE)
    points = pca.fit_transform(x_scaled)
    rows = []
    labels_by_k = {}
    for k in [2, 3, 4, 5]:
        model = KMeans(n_clusters=k, random_state=RANDOM_STATE, n_init=20)
        labels = model.fit_predict(x_scaled)
        labels_by_k[k] = labels
        rows.append({
            "k": k,
            "silhouette_score": silhouette_score(x_scaled, labels),
            "inertia": model.inertia_,
            "adjusted_rand_index": adjusted_rand_score(y, labels),
        })

    df = pd.DataFrame(rows)
    df.to_csv(RESULTS_DIR / "clustering_results.csv", index=False)
    write_json(RESULTS_DIR / "clustering_results.json", df.to_dict(orient="records"))
    best_k = int(df.sort_values("silhouette_score", ascending=False).iloc[0]["k"])
    best_labels = labels_by_k[best_k]

    plt.figure(figsize=(8, 6))
    scatter = plt.scatter(points[:, 0], points[:, 1], c=best_labels, cmap="viridis", s=24)
    plt.title(f"KMeans PCA Projection (k={best_k})")
    plt.xlabel("PCA 1")
    plt.ylabel("PCA 2")
    plt.colorbar(scatter, label="Cluster")
    plt.tight_layout()
    plt.savefig(RESULTS_DIR / "kmeans_pca_clusters.png", dpi=160)
    plt.close()

    plt.figure(figsize=(8, 6))
    scatter = plt.scatter(points[:, 0], points[:, 1], c=y, cmap="coolwarm", s=24)
    plt.title("PCA Projection Colored By True Credit Label")
    plt.xlabel("PCA 1")
    plt.ylabel("PCA 2")
    plt.colorbar(scatter, label="kredit")
    plt.tight_layout()
    plt.savefig(RESULTS_DIR / "kmeans_true_labels.png", dpi=160)
    plt.close()

    plt.figure(figsize=(7, 5))
    plt.plot(df["k"], df["inertia"], marker="o")
    plt.title("KMeans Elbow Method")
    plt.xlabel("k")
    plt.ylabel("Inertia")
    plt.tight_layout()
    plt.savefig(RESULTS_DIR / "kmeans_elbow.png", dpi=160)
    plt.close()

    plt.figure(figsize=(7, 5))
    plt.plot(df["k"], df["silhouette_score"], marker="o", color="#16a34a")
    plt.title("KMeans Silhouette Scores")
    plt.xlabel("k")
    plt.ylabel("Silhouette score")
    plt.tight_layout()
    plt.savefig(RESULTS_DIR / "kmeans_silhouette.png", dpi=160)
    plt.close()

    point_rows = pd.DataFrame({
        "x": points[:, 0],
        "y": points[:, 1],
        "cluster": best_labels,
        "true_label": y.to_numpy(),
    })
    point_rows.sample(min(200, len(point_rows)), random_state=RANDOM_STATE).to_json(
        RESULTS_DIR / "clustering_points.json",
        orient="records",
        indent=2,
    )
    return df


def save_artifacts(best_name, trained, comparison, feature_importance, x_train, feature_names):
    best_model = trained[best_name]
    joblib.dump(best_model, MODELS_DIR / "best_credit_model.pkl")
    joblib.dump(best_model, MODELS_DIR / "credit_risk_model.pkl")
    scaler = StandardScaler().fit(x_train)
    joblib.dump(scaler, MODELS_DIR / "scaler.pkl")
    joblib.dump(scaler, MODELS_DIR / "preprocessor.pkl")
    joblib.dump(feature_names, MODELS_DIR / "feature_columns.pkl")

    best_row = comparison.iloc[0].to_dict()
    metadata = {
        "selected_model_name": best_name,
        "training_date": datetime.now(timezone.utc).isoformat(),
        "dataset_name": "German Credit dataset",
        "target": TARGET,
        "target_mapping": {"0": "bad/default risk", "1": "good/non-default"},
        "feature_count": len(feature_names),
        "feature_columns": feature_names,
        "metrics": {
            "accuracy": best_row["test_accuracy"],
            "precision": best_row["test_precision"],
            "recall": best_row["test_recall"],
            "f1": best_row["test_f1"],
            "roc_auc": best_row["roc_auc"],
        },
        "best_hyperparameters": json.loads(best_row["best_hyperparameters"]),
        "selected_features": feature_importance.head(10)["feature"].tolist(),
    }
    write_json(MODELS_DIR / "model_metadata.json", metadata)
    write_json(RESULTS_DIR / "model_metadata.json", metadata)


def main():
    ensure_dirs()
    data = pd.read_csv(DATA_PATH)
    missing = data.isna().sum()
    x = data.drop(columns=[TARGET])
    y = data[TARGET]
    feature_names = list(x.columns)
    class_distribution = y.value_counts().sort_index().to_dict()

    x_train, x_test, y_train, y_test = train_test_split(
        x,
        y,
        test_size=0.2,
        stratify=y,
        random_state=RANDOM_STATE,
    )

    comparison, trained = train_and_compare(x_train, x_test, y_train, y_test)
    feature_importance, feature_results = feature_selection(x_train, x_test, y_train, y_test, feature_names)
    nn_results = neural_networks(x_train, x_test, y_train, y_test)
    clustering = cluster_experiments(x, y)
    best_name = str(comparison.iloc[0]["model"])
    save_artifacts(best_name, trained, comparison, feature_importance, x_train, feature_names)

    summary = {
        "dataset_shape": list(data.shape),
        "missing_values": missing.to_dict(),
        "numerical_features": feature_names,
        "categorical_features": [
            "laufkont", "moral", "verw", "sparkont", "beszeit", "famges", "buerge",
            "verm", "weitkred", "wohn", "beruf", "pers", "telef", "gastarb"
        ],
        "target": TARGET,
        "class_distribution": class_distribution,
        "train_rows": len(x_train),
        "test_rows": len(x_test),
        "best_model": best_name,
        "best_model_f1": float(comparison.iloc[0]["test_f1"]),
        "best_feature_selection": feature_results.iloc[0].to_dict(),
        "best_neural_network": nn_results.sort_values("f1", ascending=False).iloc[0].to_dict(),
        "best_cluster_k": int(clustering.sort_values("silhouette_score", ascending=False).iloc[0]["k"]),
    }
    write_json(RESULTS_DIR / "experiment_summary.json", summary)
    print(json.dumps(summary, indent=2, default=json_default))


if __name__ == "__main__":
    main()
