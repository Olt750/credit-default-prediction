import { apiFetch } from "@/lib/api";

export type ModelComparisonRow = {
  model: string;
  tested_hyperparameters: string;
  best_hyperparameters: string;
  cv_score: number;
  baseline_accuracy: number;
  baseline_precision: number;
  baseline_recall: number;
  baseline_f1: number;
  test_accuracy: number;
  test_precision: number;
  test_recall: number;
  test_f1: number;
  roc_auc: number;
  confusion_matrix: { tn: number; fp: number; fn: number; tp: number; labels: string[] };
};

export type FeatureImportanceRow = {
  feature: string;
  description: string;
  importance: string | number;
};

export type NeuralNetworkRow = {
  architecture: string;
  hidden_layer_sizes: number[];
  activation: string;
  solver: string;
  learning_rate_init: number;
  layers: string[];
  training_time_seconds: number;
  accuracy: number;
  precision: number;
  recall: number;
  f1: number;
  roc_auc: number;
  confusion_matrix: { tn: number; fp: number; fn: number; tp: number; labels: string[] };
};

export type ClusteringRow = {
  k: number;
  silhouette_score: number;
  inertia: number;
  adjusted_rand_index: number;
};

export type ClusterPoint = {
  x: number;
  y: number;
  cluster: number;
  true_label: number;
};

export type ModelMetadata = {
  selected_model_name: string;
  training_date: string;
  dataset_name: string;
  feature_count: number;
  metrics: Record<string, number>;
  best_hyperparameters: Record<string, unknown>;
  selected_features: string[];
};

export async function getMlResult<T>(path: string): Promise<T> {
  const response = await apiFetch(`/ml/${path}`);
  if (!response.ok) {
    const error = await response.json().catch(() => ({ error: "Unable to load ML results." }));
    throw new Error(error.error ?? "Unable to load ML results.");
  }

  return response.json();
}
