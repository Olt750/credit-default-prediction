import { createFileRoute } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { Download, FileSpreadsheet, Sparkles, Upload } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { downloadApiFile, generateReport, getReports, importData } from "@/services/additionalFeaturesApi";

export const Route = createFileRoute("/_app/reports")({
  component: ReportsPage,
});

const reportTypes = [
  "Prediction Summary Report",
  "Risk Distribution Report",
  "User Prediction History Report",
  "High Risk Applications Report",
  "Model Performance Report",
];

const dataTypes = ["predictions", "client-profiles", "notifications", "reports", "users"];
const formats = ["csv", "xlsx", "json"];

type Report = {
  id: string;
  name: string;
  reportType: string;
  format?: string;
  createdAt: string;
  summary?: {
    totalPredictions: number;
    lowRiskCount: number;
    mediumRiskCount: number;
    highRiskCount: number;
    averageRiskScore: number;
  };
};

function ReportsPage() {
  const [reports, setReports] = useState<Report[]>([]);
  const [selectedReport, setSelectedReport] = useState<Report | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [importResult, setImportResult] = useState<any>(null);
  const [filters, setFilters] = useState({
    dateFrom: "",
    dateTo: "",
    riskLevel: "",
    minRiskScore: "",
    maxRiskScore: "",
    reportType: "Prediction Summary Report",
    format: "csv",
  });
  const [tool, setTool] = useState({ dataType: "predictions", format: "csv" });

  async function loadReports() {
    const data = await getReports({ page: 1, pageSize: 20, sortBy: "createdAt", sortDirection: "desc" });
    setReports(data.items ?? []);
  }

  useEffect(() => {
    loadReports().catch((err) => setError(err.message));
  }, []);

  async function onGenerate() {
    setLoading(true);
    setError(null);
    try {
      const report = await generateReport({
        ...filters,
        minRiskScore: filters.minRiskScore ? Number(filters.minRiskScore) : undefined,
        maxRiskScore: filters.maxRiskScore ? Number(filters.maxRiskScore) : undefined,
      });
      setSelectedReport(report);
      await loadReports();
    } catch (err: any) {
      setError(err.message || "Report generation failed.");
    } finally {
      setLoading(false);
    }
  }

  async function onImport(file?: File) {
    if (!file) return;
    setError(null);
    try {
      setImportResult(await importData(tool.dataType, file));
    } catch (err: any) {
      setError(err.message || "Import failed.");
    }
  }

  const summary = selectedReport?.summary;

  return (
    <>
      <PageHeader title="Reports" subtitle="Generate dynamic reports, export real data, and import validated records." />

      {error && <div className="mb-4 rounded-xl border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">{error}</div>}

      <div className="grid xl:grid-cols-[360px_1fr] gap-4">
        <div className="space-y-4">
          <div className="glass rounded-2xl p-6 space-y-4">
            <Select label="Report type" value={filters.reportType} options={reportTypes} onChange={(value) => setFilters({ ...filters, reportType: value })} />
            <div className="grid grid-cols-2 gap-3">
              <Field label="From date" type="date" value={filters.dateFrom} onChange={(value) => setFilters({ ...filters, dateFrom: value })} />
              <Field label="To date" type="date" value={filters.dateTo} onChange={(value) => setFilters({ ...filters, dateTo: value })} />
            </div>
            <Select label="Risk level" value={filters.riskLevel} options={["", "Low", "Medium", "High"]} onChange={(value) => setFilters({ ...filters, riskLevel: value })} />
            <div className="grid grid-cols-2 gap-3">
              <Field label="Min score" type="number" value={filters.minRiskScore} onChange={(value) => setFilters({ ...filters, minRiskScore: value })} />
              <Field label="Max score" type="number" value={filters.maxRiskScore} onChange={(value) => setFilters({ ...filters, maxRiskScore: value })} />
            </div>
            <Select label="Format" value={filters.format} options={formats} onChange={(value) => setFilters({ ...filters, format: value })} />
            <button disabled={loading} onClick={onGenerate} className="w-full h-11 rounded-xl gradient-hero text-sm font-medium text-white shadow-[var(--shadow-glow)] flex items-center justify-center gap-2 disabled:opacity-60">
              <Sparkles className="size-4" /> {loading ? "Generating..." : "Generate Report"}
            </button>
          </div>

          <div className="glass rounded-2xl p-6 space-y-4">
            <div className="flex items-center gap-2 font-semibold"><FileSpreadsheet className="size-4 text-primary" /> Export / Import</div>
            <Select label="Data type" value={tool.dataType} options={dataTypes} onChange={(value) => setTool({ ...tool, dataType: value })} />
            <Select label="Format" value={tool.format} options={formats} onChange={(value) => setTool({ ...tool, format: value })} />
            <button onClick={() => downloadApiFile(`/export/${tool.dataType}?format=${tool.format}`).catch((err) => setError(err.message))} className="w-full h-10 rounded-xl border border-border text-sm hover:bg-muted/40 flex items-center justify-center gap-2">
              <Download className="size-4" /> Export
            </button>
            <label className="w-full h-10 rounded-xl border border-border text-sm hover:bg-muted/40 flex items-center justify-center gap-2 cursor-pointer">
              <Upload className="size-4" /> Import CSV/JSON
              <input className="hidden" type="file" accept=".csv,.json" onChange={(e) => onImport(e.target.files?.[0])} />
            </label>
            {importResult && (
              <div className="rounded-xl bg-muted/40 p-3 text-xs text-muted-foreground">
                Inserted {importResult.insertedRows}, skipped {importResult.skippedRows}, failed {importResult.failedRows}.
              </div>
            )}
          </div>
        </div>

        <div className="space-y-4">
          <div className="glass rounded-2xl p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-semibold">Report summary</h3>
              {selectedReport && <button onClick={() => downloadApiFile(`/reports/${selectedReport.id}/download`).catch((err) => setError(err.message))} className="h-9 px-3 rounded-lg border border-border text-sm hover:bg-muted/40">Download</button>}
            </div>
            {summary ? (
              <div className="grid sm:grid-cols-5 gap-3">
                <Summary label="Total" value={summary.totalPredictions} />
                <Summary label="Low" value={summary.lowRiskCount} />
                <Summary label="Medium" value={summary.mediumRiskCount} />
                <Summary label="High" value={summary.highRiskCount} />
                <Summary label="Avg score" value={summary.averageRiskScore} />
              </div>
            ) : (
              <div className="text-sm text-muted-foreground">Generate a report to preview its summary.</div>
            )}
          </div>

          <div className="bg-card border border-border rounded-2xl shadow-[var(--shadow-card)] overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs uppercase tracking-wider text-muted-foreground border-b border-border bg-muted/40">
                  <th className="py-3 px-5 font-medium">Name</th>
                  <th className="py-3 pr-4 font-medium">Type</th>
                  <th className="py-3 pr-4 font-medium">Format</th>
                  <th className="py-3 pr-4 font-medium">Created</th>
                  <th className="py-3 pr-5 font-medium text-right">Download</th>
                </tr>
              </thead>
              <tbody>
                {reports.map((report) => (
                  <tr key={report.id} className="border-b border-border last:border-0 hover:bg-muted/30">
                    <td className="py-3 px-5 font-medium">{report.name}</td>
                    <td className="py-3 pr-4 text-muted-foreground">{report.reportType}</td>
                    <td className="py-3 pr-4 uppercase">{report.format}</td>
                    <td className="py-3 pr-4 text-muted-foreground">{new Date(report.createdAt).toLocaleString()}</td>
                    <td className="py-3 pr-5 text-right">
                      <button onClick={() => downloadApiFile(`/reports/${report.id}/download`).catch((err) => setError(err.message))} className="text-primary text-xs">Download</button>
                    </td>
                  </tr>
                ))}
                {!reports.length && <tr><td colSpan={5} className="py-6 px-5 text-center text-muted-foreground">No reports generated yet.</td></tr>}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </>
  );
}

function Field({ label, value, type, onChange }: { label: string; value: string; type: string; onChange: (value: string) => void }) {
  return (
    <div>
      <label className="text-xs text-muted-foreground">{label}</label>
      <input type={type} value={value} onChange={(e) => onChange(e.target.value)} className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm" />
    </div>
  );
}

function Select({ label, value, options, onChange }: { label: string; value: string; options: string[]; onChange: (value: string) => void }) {
  return (
    <div>
      <label className="text-xs text-muted-foreground">{label}</label>
      <select value={value} onChange={(e) => onChange(e.target.value)} className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
        {options.map((option) => <option key={option} value={option}>{option || "All"}</option>)}
      </select>
    </div>
  );
}

function Summary({ label, value }: { label: string; value: number }) {
  return <div className="rounded-xl bg-muted/40 p-3"><div className="text-xs text-muted-foreground">{label}</div><div className="text-xl font-semibold mt-1">{value}</div></div>;
}
