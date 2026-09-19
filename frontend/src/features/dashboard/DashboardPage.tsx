import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { httpClient } from "../../api/httpClient";
import { getSession, requireHouseholdId } from "../../api/session";
import { PageHeader } from "../../components/PageHeader";
import { DashboardSummary, ReceiptSummary, UpcomingBill } from "../../types";
import { formatCurrency, formatDate } from "../../format";

export function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [recentReceipts, setRecentReceipts] = useState<ReceiptSummary[]>([]);
  const [upcomingBills, setUpcomingBills] = useState<UpcomingBill[]>([]);
  const firstName = getSession()?.displayName?.split(" ")[0];

  useEffect(() => {
    const householdId = requireHouseholdId();
    const today = new Date();
    const from = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().slice(0, 10);
    const to = today.toISOString().slice(0, 10);

    httpClient
      .get<DashboardSummary>(`/households/${householdId}/dashboard-summary?from=${from}&to=${to}`)
      .then(setSummary)
      .catch(() => setSummary(null));

    httpClient
      .get<{ items: ReceiptSummary[] }>(`/households/${householdId}/receipts?page=1&pageSize=5`)
      .then((result) => setRecentReceipts(result.items))
      .catch(() => setRecentReceipts([]));

    httpClient
      .get<UpcomingBill[]>(`/households/${householdId}/bills`)
      .then((bills) => setUpcomingBills(bills.filter((bill) => !bill.isPaid).slice(0, 3)))
      .catch(() => setUpcomingBills([]));
  }, []);

  return (
    <>
      <PageHeader
        title="Dashboard"
        description={`Welcome back${firstName ? `, ${firstName}` : ""} — here's the household's spending.`}
        actions={
          <Link to="/upload" className="btn btn-primary">
            <i className="ki-filled ki-cloud-add" />
            Upload Receipts
          </Link>
        }
      />

      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-5 mb-5">
        <KpiCard
          icon="ki-wallet"
          tone="primary"
          label="Total Spent"
          value={formatCurrency(summary?.totalSpent ?? 0)}
          hint="This month"
        />
        <KpiCard
          icon="ki-bill"
          tone="info"
          label="Receipts Uploaded"
          value={String(summary?.receiptCount ?? 0)}
          hint="This month"
        />
        <KpiCard
          icon="ki-chart-simple"
          tone="warning"
          label="Avg / Receipt"
          value={formatCurrency(summary?.averagePerReceipt ?? 0)}
          hint={`Across ${summary?.receiptCount ?? 0} receipts`}
        />
        <KpiCard
          icon="ki-tag"
          tone="success"
          label="Top Category"
          value={topCategory(summary)?.[0] ?? "—"}
          hint={formatCurrency(topCategory(summary)?.[1] ?? 0)}
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Recent Receipts</h3>
            <Link to="/receipts" className="btn btn-sm btn-light">
              View all
              <i className="ki-filled ki-right" />
            </Link>
          </div>
          <div className="card-body py-2">
            {recentReceipts.map((receipt) => (
              <Link
                key={receipt.id}
                to={`/receipts/${receipt.id}`}
                className="flex items-center gap-3.5 py-3 border-b border-gray-200 last:border-b-0"
              >
                <span className="size-10 rounded-lg bg-primary-light text-primary flex items-center justify-center shrink-0">
                  <i className="ki-filled ki-bill text-xl" />
                </span>
                <div className="grow min-w-0">
                  <div className="text-sm font-medium text-gray-900 truncate">{receipt.merchant}</div>
                  <div className="text-xs text-gray-600">{formatDate(receipt.purchasedOn)}</div>
                </div>
                <div className="text-sm font-semibold text-gray-900">{formatCurrency(receipt.total)}</div>
              </Link>
            ))}
            {recentReceipts.length === 0 && <EmptyState icon="ki-bill" message="No receipts yet." />}
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Upcoming Bills</h3>
            <Link to="/bills" className="btn btn-sm btn-light">
              View all
              <i className="ki-filled ki-right" />
            </Link>
          </div>
          <div className="card-body py-2">
            {upcomingBills.map((bill) => (
              <div key={bill.id} className="flex items-center gap-3.5 py-3 border-b border-gray-200 last:border-b-0">
                <span className={`size-10 rounded-lg flex items-center justify-center shrink-0 ${toneBox(bill.daysUntilDue)}`}>
                  <i className="ki-filled ki-calendar-tick text-xl" />
                </span>
                <div className="grow min-w-0">
                  <div className="text-sm font-medium text-gray-900 truncate">{bill.name}</div>
                  <div className={`text-xs ${toneText(bill.daysUntilDue)}`}>{dueLabel(bill.daysUntilDue)}</div>
                </div>
                <div className="text-sm font-semibold text-gray-900">{formatCurrency(bill.amount)}</div>
              </div>
            ))}
            {upcomingBills.length === 0 && <EmptyState icon="ki-calendar-tick" message="Nothing due soon." />}
          </div>
        </div>
      </div>
    </>
  );
}

const TONE_STYLES = {
  primary: "bg-primary-light text-primary",
  info: "bg-info-light text-info",
  warning: "bg-warning-light text-warning",
  success: "bg-success-light text-success"
} as const;

function KpiCard({
  icon,
  tone,
  label,
  value,
  hint
}: {
  icon: string;
  tone: keyof typeof TONE_STYLES;
  label: string;
  value: string;
  hint: string;
}) {
  return (
    <div className="card">
      <div className="card-body flex items-center gap-4">
        <span className={`size-12 rounded-xl flex items-center justify-center shrink-0 ${TONE_STYLES[tone]}`}>
          <i className={`ki-filled ${icon} text-2xl`} />
        </span>
        <div className="min-w-0">
          <div className="text-2xs font-medium uppercase tracking-wide text-gray-600">{label}</div>
          <div className="text-xl font-semibold text-gray-900 truncate mt-1">{value}</div>
          <div className="text-xs text-gray-500 mt-0.5">{hint}</div>
        </div>
      </div>
    </div>
  );
}

function EmptyState({ icon, message }: { icon: string; message: string }) {
  return (
    <div className="flex flex-col items-center gap-2 py-8 text-gray-500">
      <i className={`ki-filled ${icon} text-3xl`} />
      <span className="text-2sm">{message}</span>
    </div>
  );
}

function topCategory(summary: DashboardSummary | null): [string, number] | undefined {
  if (!summary) {
    return undefined;
  }

  return Object.entries(summary.spendByCategory).sort((a, b) => b[1] - a[1])[0];
}

function toneBox(daysUntilDue: number): string {
  if (daysUntilDue < 0) return "bg-danger-light text-danger";
  if (daysUntilDue <= 2) return "bg-warning-light text-warning";
  return "bg-primary-light text-primary";
}

function toneText(daysUntilDue: number): string {
  if (daysUntilDue < 0) return "text-danger";
  if (daysUntilDue <= 2) return "text-warning";
  return "text-gray-600";
}

function dueLabel(daysUntilDue: number): string {
  if (daysUntilDue < 0) return `Overdue by ${Math.abs(daysUntilDue)} days`;
  if (daysUntilDue === 0) return "Due today";
  if (daysUntilDue === 1) return "Due tomorrow";
  return `Due in ${daysUntilDue} days`;
}
