import { FormEvent, useEffect, useRef, useState } from "react";
import { httpClient } from "../../api/httpClient";
import { ensureConnected } from "../../api/signalrClient";
import { getSession, requireHouseholdId } from "../../api/session";
import { PageHeader } from "../../components/PageHeader";
import { ReminderDueUpdate, UpcomingBill } from "../../types";
import { formatCurrency } from "../../format";

const RECURRENCES = ["OneTime", "Weekly", "Monthly", "Quarterly", "Yearly"] as const;
const CATEGORIES = ["Loan", "Utility", "Subscription", "Rental", "Insurance", "Other"];
const NOTIFY_OPTIONS = [1, 3, 7];

interface DraftBill {
  name: string;
  amount: string;
  dueDate: string;
  recurrence: (typeof RECURRENCES)[number];
  category: string;
  notifyDaysBefore: number;
}

const EMPTY_DRAFT: DraftBill = {
  name: "",
  amount: "",
  dueDate: "",
  recurrence: "Monthly",
  category: "Loan",
  notifyDaysBefore: 3
};

export function BillsPage() {
  const [bills, setBills] = useState<UpcomingBill[]>([]);
  const [draft, setDraft] = useState<DraftBill>(EMPTY_DRAFT);
  const [saving, setSaving] = useState(false);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [editingId, setEditingId] = useState<string | null>(null);
  const formRef = useRef<HTMLFormElement>(null);

  const householdId = requireHouseholdId();

  function refresh() {
    httpClient.get<UpcomingBill[]>(`/households/${householdId}/bills`).then((result) => {
      setBills(result);
      setSelected((current) => new Set(result.filter((bill) => current.has(bill.id)).map((bill) => bill.id)));
    });
  }

  const allSelected = bills.length > 0 && bills.every((bill) => selected.has(bill.id));

  function toggleOne(id: string) {
    setSelected((current) => {
      const next = new Set(current);
      if (!next.delete(id)) next.add(id);
      return next;
    });
  }

  function toggleAll() {
    setSelected(allSelected ? new Set() : new Set(bills.map((bill) => bill.id)));
  }

  async function handleBulkDelete() {
    const count = selected.size;
    if (!window.confirm(`Delete ${count} bill${count > 1 ? "s" : ""}? This cannot be undone.`)) return;

    await httpClient.post(`/households/${householdId}/bills/bulk-delete`, { ids: [...selected] });
    if (editingId && selected.has(editingId)) cancelEdit();
    refresh();
  }

  function startEdit(bill: UpcomingBill) {
    setEditingId(bill.id);
    setDraft({
      name: bill.name,
      amount: String(bill.amount),
      dueDate: bill.dueDate.slice(0, 10),
      recurrence: bill.recurrence,
      category: bill.category,
      notifyDaysBefore: bill.notifyDaysBefore ?? EMPTY_DRAFT.notifyDaysBefore
    });
    formRef.current?.scrollIntoView({ behavior: "smooth", block: "nearest" });
  }

  function cancelEdit() {
    setEditingId(null);
    setDraft(EMPTY_DRAFT);
  }

  useEffect(() => {
    refresh();

    let disposed = false;
    ensureConnected().then((hub) => {
      if (disposed) return;
      hub.on("ReminderDue", (_update: ReminderDueUpdate) => refresh());
    });

    return () => {
      disposed = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function handleMarkPaid(billId: string) {
    await httpClient.post(`/households/${householdId}/bills/${billId}/mark-paid`);
    refresh();
  }

  async function handleDelete(bill: UpcomingBill) {
    if (!window.confirm(`Delete "${bill.name}"? This cannot be undone.`)) return;

    await httpClient.delete(`/households/${householdId}/bills/${bill.id}`);
    if (editingId === bill.id) cancelEdit();
    refresh();
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);

    const fields = {
      name: draft.name,
      amount: Number(draft.amount),
      dueDate: draft.dueDate,
      recurrence: draft.recurrence,
      category: draft.category,
      notifyDaysBefore: draft.notifyDaysBefore
    };

    try {
      if (editingId) {
        await httpClient.put(`/households/${householdId}/bills/${editingId}`, fields);
      } else {
        await httpClient.post(`/households/${householdId}/bills`, {
          createdByUserId: getSession()?.userId,
          ...fields
        });
      }

      cancelEdit();
      refresh();
    } finally {
      setSaving(false);
    }
  }

  const unpaid = bills.filter((bill) => !bill.isPaid);
  const attentionCount = unpaid.filter((bill) => bill.daysUntilDue <= 3).length;
  const attentionTotal = unpaid
    .filter((bill) => bill.daysUntilDue <= 3)
    .reduce((sum, bill) => sum + bill.amount, 0);

  return (
    <>
      <PageHeader
        title="Bills & Loans"
        description="Set a reminder once — Resit pings you in-app as the due date nears."
      />

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-5 items-start">
        <div className="xl:col-span-2 flex flex-col gap-5">
          {attentionCount > 0 && (
            <div className="flex items-center gap-3.5 rounded-xl border border-warning-clarity bg-warning-light px-5 py-4">
              <span className="size-10 rounded-lg bg-warning text-warning-inverse flex items-center justify-center shrink-0">
                <i className="ki-filled ki-notification-status text-xl" />
              </span>
              <div>
                <div className="text-sm font-semibold text-gray-900">
                  {attentionCount} bill{attentionCount > 1 ? "s" : ""} need attention this week
                </div>
                <div className="text-2sm text-gray-700">{formatCurrency(attentionTotal)} total due</div>
              </div>
            </div>
          )}

          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Your bills</h3>
              <div className="flex items-center gap-3.5">
                {selected.size > 0 && (
                  <button type="button" className="btn btn-sm btn-outline btn-danger" onClick={handleBulkDelete}>
                    <i className="ki-filled ki-trash" />
                    Delete selected ({selected.size})
                  </button>
                )}
                {bills.length > 0 && (
                  <label className="flex items-center gap-2 text-2sm text-gray-600 cursor-pointer">
                    <input type="checkbox" className="checkbox checkbox-sm" checked={allSelected} onChange={toggleAll} />
                    Select all
                  </label>
                )}
                <span className="text-2sm text-gray-600">{unpaid.length} unpaid</span>
              </div>
            </div>
            <div className="card-body py-2">
              {bills.map((bill) => (
                <div
                  key={bill.id}
                  className={`flex items-center gap-3.5 py-3.5 border-b border-gray-200 last:border-b-0 ${bill.isPaid ? "opacity-60" : ""} ${
                    editingId === bill.id ? "bg-primary-light -mx-3 px-3 rounded-lg" : ""
                  }`}
                >
                  <input
                    type="checkbox"
                    className="checkbox checkbox-sm shrink-0"
                    checked={selected.has(bill.id)}
                    onChange={() => toggleOne(bill.id)}
                    aria-label={`Select ${bill.name}`}
                  />
                  <span className={`size-10 rounded-lg flex items-center justify-center shrink-0 ${iconBox(bill)}`} aria-hidden="true">
                    <i className={`ki-filled ${bill.isPaid ? "ki-check-circle" : "ki-calendar-tick"} text-xl`} />
                  </span>
                  <div className="grow min-w-0">
                    <div className="text-sm font-medium text-gray-900 truncate">{bill.name}</div>
                    <div className="text-xs text-gray-600">
                      {bill.category} · {bill.recurrence}
                    </div>
                  </div>
                  <div className="flex flex-col items-end gap-2 sm:flex-row sm:items-center sm:gap-3.5">
                    <div className="text-end">
                      <div className="text-sm font-semibold text-gray-900">{formatCurrency(bill.amount)}</div>
                      <div className={`text-xs ${dueTone(bill)}`}>{dueLabel(bill)}</div>
                    </div>
                    {!bill.isPaid && (
                      <button
                        type="button"
                        className="btn btn-sm btn-outline btn-primary whitespace-nowrap"
                        onClick={() => handleMarkPaid(bill.id)}
                      >
                        <i className="ki-filled ki-check" />
                        Mark Paid
                      </button>
                    )}
                    <button
                      type="button"
                      className="btn btn-sm btn-icon btn-light"
                      onClick={() => startEdit(bill)}
                      aria-label={`Edit ${bill.name}`}
                    >
                      <i className="ki-filled ki-pencil" />
                    </button>
                    <button
                      type="button"
                      className="btn btn-sm btn-icon btn-outline btn-danger"
                      onClick={() => handleDelete(bill)}
                      aria-label={`Delete ${bill.name}`}
                    >
                      <i className="ki-filled ki-trash" />
                    </button>
                  </div>
                </div>
              ))}
              {bills.length === 0 && (
                <div className="flex flex-col items-center gap-2 py-10 text-gray-500">
                  <i className="ki-filled ki-calendar-tick text-3xl" />
                  <span className="text-2sm">No bills yet — add your first reminder.</span>
                </div>
              )}
            </div>
          </div>
        </div>

        <form ref={formRef} className="card" onSubmit={handleSubmit}>
          <div className="card-header">
            <h3 className="card-title">{editingId ? "Edit Reminder" : "Add a Reminder"}</h3>
            {editingId && (
              <button type="button" className="btn btn-sm btn-light" onClick={cancelEdit}>
                Cancel
              </button>
            )}
          </div>

          <div className="card-body flex flex-col gap-5">
            <div className="flex flex-col gap-1">
              <label htmlFor="name" className="form-label">
                Name
              </label>
              <input
                id="name"
                className="input"
                value={draft.name}
                onChange={(event) => setDraft({ ...draft, name: event.target.value })}
                placeholder="e.g. TNB Electricity"
                required
              />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-1 2xl:grid-cols-2 gap-5">
              <div className="flex flex-col gap-1">
                <label htmlFor="amount" className="form-label">
                  Amount (RM)
                </label>
                <input
                  id="amount"
                  className="input"
                  type="number"
                  step="0.01"
                  value={draft.amount}
                  onChange={(event) => setDraft({ ...draft, amount: event.target.value })}
                  required
                />
              </div>
              <div className="flex flex-col gap-1">
                <label htmlFor="dueDate" className="form-label">
                  Due Date
                </label>
                <input
                  id="dueDate"
                  className="input"
                  type="date"
                  value={draft.dueDate}
                  onChange={(event) => setDraft({ ...draft, dueDate: event.target.value })}
                  required
                />
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-1 2xl:grid-cols-2 gap-5">
              <div className="flex flex-col gap-1">
                <label htmlFor="recurrence" className="form-label">
                  Repeats
                </label>
                <select
                  id="recurrence"
                  className="select"
                  value={draft.recurrence}
                  onChange={(event) => setDraft({ ...draft, recurrence: event.target.value as DraftBill["recurrence"] })}
                >
                  {RECURRENCES.map((option) => (
                    <option key={option} value={option}>
                      {option === "OneTime" ? "One-time" : option}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex flex-col gap-1">
                <label htmlFor="category" className="form-label">
                  Category
                </label>
                <select
                  id="category"
                  className="select"
                  value={draft.category}
                  onChange={(event) => setDraft({ ...draft, category: event.target.value })}
                >
                  {(CATEGORIES.includes(draft.category) ? CATEGORIES : [...CATEGORIES, draft.category]).map((category) => (
                    <option key={category} value={category}>
                      {category}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className="flex flex-col gap-1">
              <label htmlFor="notifyDaysBefore" className="form-label">
                Notify me
              </label>
              <select
                id="notifyDaysBefore"
                className="select"
                value={draft.notifyDaysBefore}
                onChange={(event) => setDraft({ ...draft, notifyDaysBefore: Number(event.target.value) })}
              >
                {[1, 3, 7, ...(NOTIFY_OPTIONS.includes(draft.notifyDaysBefore) ? [] : [draft.notifyDaysBefore])].map((days) => (
                  <option key={days} value={days}>
                    {days} day{days > 1 ? "s" : ""} before
                  </option>
                ))}
              </select>
            </div>

            <button type="submit" className="btn btn-primary justify-center" disabled={saving}>
              <i className={`ki-filled ${editingId ? "ki-check" : "ki-plus"}`} />
              {saving ? "Saving…" : editingId ? "Save Changes" : "Save Reminder"}
            </button>
          </div>
        </form>
      </div>
    </>
  );
}

function iconBox(bill: UpcomingBill): string {
  if (bill.isPaid) return "bg-success-light text-success";
  if (bill.daysUntilDue < 0) return "bg-danger-light text-danger";
  if (bill.daysUntilDue <= 3) return "bg-warning-light text-warning";
  return "bg-primary-light text-primary";
}

function dueTone(bill: UpcomingBill): string {
  if (bill.isPaid) return "text-success";
  if (bill.daysUntilDue < 0) return "text-danger";
  if (bill.daysUntilDue <= 3) return "text-warning";
  return "text-gray-600";
}

function dueLabel(bill: UpcomingBill): string {
  if (bill.isPaid) return "Paid";
  if (bill.daysUntilDue < 0) return `Overdue by ${Math.abs(bill.daysUntilDue)} days`;
  if (bill.daysUntilDue === 0) return "Due today";
  if (bill.daysUntilDue === 1) return "Due tomorrow";
  return `Due in ${bill.daysUntilDue} days`;
}
