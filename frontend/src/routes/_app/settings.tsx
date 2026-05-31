import { createFileRoute } from "@tanstack/react-router";
import { User, Palette, Cpu, Settings as SettingsIcon } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";

export const Route = createFileRoute("/_app/settings")({
  component: SettingsPage,
});

function SettingsPage() {
  return (
    <>
      <PageHeader title="Settings" subtitle="Manage your profile, application preferences, and model configuration." />

      <div className="grid lg:grid-cols-3 gap-4">
        <div className="glass rounded-2xl p-6">
          <div className="flex items-center gap-3">
            <div className="size-14 rounded-2xl gradient-hero flex items-center justify-center text-white font-semibold">AR</div>
            <div>
              <div className="font-semibold">Anita Rama</div>
              <div className="text-xs text-muted-foreground">Risk Analyst — CreditIQ</div>
            </div>
          </div>
          <div className="mt-5 space-y-3 text-sm">
            <Field label="Email" value="anita.rama@bank.al" />
            <Field label="Role" value="Analyst" />
            <Field label="Team" value="Retail Credit" />
          </div>
        </div>

        <Card title="Theme settings" icon={Palette}>
          <ToggleRow label="Dark mode" defaultChecked />
          <ToggleRow label="Reduced motion" />
          <ToggleRow label="Compact tables" />
        </Card>

        <Card title="Model settings" icon={Cpu}>
          <SelectRow label="Default model" options={["Random Forest", "Neural Network", "Logistic Regression", "Decision Tree"]} />
          <SelectRow label="Threshold strategy" options={["Balanced", "High recall", "High precision"]} />
          <ToggleRow label="Auto-retrain weekly" defaultChecked />
        </Card>

        <Card title="Application settings" icon={SettingsIcon} className="lg:col-span-3">
          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <ToggleRow label="Email alerts" defaultChecked />
            <ToggleRow label="Slack alerts" />
            <ToggleRow label="Two-factor auth" defaultChecked />
            <ToggleRow label="API access" />
          </div>
        </Card>
      </div>
    </>
  );
}

function Card({ title, icon: Icon, children, className = "" }: { title: string; icon: any; children: React.ReactNode; className?: string }) {
  return (
    <div className={`glass rounded-2xl p-6 ${className}`}>
      <div className="flex items-center gap-2 mb-4">
        <div className="size-8 rounded-lg bg-muted/60 flex items-center justify-center"><Icon className="size-4 text-primary" /></div>
        <h3 className="font-semibold">{title}</h3>
      </div>
      <div className="space-y-3">{children}</div>
    </div>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between border-b border-border/40 pb-2">
      <span className="text-muted-foreground">{label}</span>
      <span className="font-medium">{value}</span>
    </div>
  );
}

function ToggleRow({ label, defaultChecked }: { label: string; defaultChecked?: boolean }) {
  return (
    <label className="flex items-center justify-between gap-3 cursor-pointer">
      <span className="text-sm">{label}</span>
      <span className="relative inline-flex">
        <input type="checkbox" defaultChecked={defaultChecked} className="peer sr-only" />
        <span className="w-10 h-6 rounded-full bg-muted peer-checked:bg-primary transition-colors" />
        <span className="absolute left-0.5 top-0.5 size-5 rounded-full bg-white peer-checked:translate-x-4 transition-transform" />
      </span>
    </label>
  );
}

function SelectRow({ label, options }: { label: string; options: string[] }) {
  return (
    <div>
      <label className="text-xs text-muted-foreground">{label}</label>
      <select className="mt-1 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
        {options.map((o) => <option key={o}>{o}</option>)}
      </select>
    </div>
  );
}