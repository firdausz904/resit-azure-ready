import { FormEvent, useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { httpClient } from "../../api/httpClient";
import { requireHouseholdId } from "../../api/session";
import { PageHeader } from "../../components/PageHeader";
import { formatCurrency } from "../../format";

interface ReceiptDetail {
  id: string;
  fileName: string;
  merchant: string;
  total: number;
  purchasedOn: string | null;
  category: string;
  status: string;
  merchantConfidence: number | null;
  totalConfidence: number | null;
  dateConfidence: number | null;
  rawOcrText: string | null;
}

const CATEGORIES = ["Groceries", "Dining", "Transport", "Utilities", "Health", "Other"];

export function ReceiptDetailPage() {
  const { receiptId } = useParams<{ receiptId: string }>();
  const navigate = useNavigate();
  const [receipt, setReceipt] = useState<ReceiptDetail | null>(null);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (!receiptId) return;

    const householdId = requireHouseholdId();
    httpClient
      .get<ReceiptDetail>(`/households/${householdId}/receipts/${receiptId}`)
      .then(setReceipt)
      .catch(() => setReceipt(null));
  }, [receiptId]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!receipt) return;

    setSaving(true);
    const householdId = requireHouseholdId();

    try {
      await httpClient.put(`/households/${householdId}/receipts/${receipt.id}`, {
        merchant: receipt.merchant,
        total: receipt.total,
        purchasedOn: receipt.purchasedOn,
        category: receipt.category
      });
      navigate("/receipts");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    if (!receipt) return;
    if (!window.confirm("Delete this receipt? This cannot be undone.")) return;

    setDeleting(true);

    try {
      await httpClient.delete(`/households/${requireHouseholdId()}/receipts/${receipt.id}`);
      navigate("/receipts");
    } finally {
      setDeleting(false);
    }
  }

  if (!receipt) {
    return <p className="text-2sm text-gray-600">Loading receipt…</p>;
  }

  return (
    <>
      <PageHeader
        title="Receipt Details"
        description={receipt.fileName}
        actions={
          <Link to="/receipts" className="btn btn-light">
            <i className="ki-filled ki-arrow-left" />
            Back to Receipts
          </Link>
        }
      />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5 items-start">
        <div className="rounded-xl bg-gray-100 border border-gray-200 flex items-center justify-center p-8">
          <div className="w-[280px] bg-light shadow-lg p-6">
            <div className="text-center font-bold text-sm text-gray-900 mb-3.5 break-words">
              {receipt.merchant.toUpperCase()}
            </div>
            <div className="flex flex-col gap-1.5">
              {Array.from({ length: 5 }).map((_, index) => (
                <div key={index} className="h-1.5 rounded-sm bg-gray-200" />
              ))}
            </div>
            <div className="flex justify-between text-sm font-bold text-gray-900 mt-4 pt-3.5 border-t border-dashed border-gray-400">
              <span>TOTAL</span>
              <span>{formatCurrency(receipt.total)}</span>
            </div>
          </div>
        </div>

        <form className="card" onSubmit={handleSubmit}>
          <div className="card-header">
            <h3 className="card-title">Edit extracted data</h3>
            <span className="badge badge-sm badge-outline badge-primary">
              {receipt.status === "NeedsReview" ? "Needs Review" : receipt.status}
            </span>
          </div>

          <div className="card-body flex flex-col gap-5">
            <div className="flex flex-col gap-1">
              <label htmlFor="merchant" className="form-label justify-between items-center">
                Merchant
                <Confidence value={receipt.merchantConfidence} />
              </label>
              <input
                id="merchant"
                className="input"
                value={receipt.merchant}
                onChange={(event) => setReceipt({ ...receipt, merchant: event.target.value })}
              />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              <div className="flex flex-col gap-1">
                <label htmlFor="total" className="form-label justify-between items-center">
                  Total Amount
                  <Confidence value={receipt.totalConfidence} />
                </label>
                <input
                  id="total"
                  className="input"
                  type="number"
                  step="0.01"
                  value={receipt.total}
                  onChange={(event) => setReceipt({ ...receipt, total: Number(event.target.value) })}
                />
              </div>
              <div className="flex flex-col gap-1">
                <label htmlFor="purchasedOn" className="form-label justify-between items-center">
                  Date
                  <Confidence value={receipt.dateConfidence} />
                </label>
                <input
                  id="purchasedOn"
                  className="input"
                  type="date"
                  value={receipt.purchasedOn ?? ""}
                  onChange={(event) => setReceipt({ ...receipt, purchasedOn: event.target.value })}
                />
              </div>
            </div>

            <div className="flex flex-col gap-1">
              <label htmlFor="category" className="form-label">
                Category
              </label>
              <select
                id="category"
                className="select"
                value={receipt.category}
                onChange={(event) => setReceipt({ ...receipt, category: event.target.value })}
              >
                {CATEGORIES.map((category) => (
                  <option key={category} value={category}>
                    {category}
                  </option>
                ))}
              </select>
            </div>

            {receipt.rawOcrText && (
              <details className="text-2sm text-gray-600">
                <summary className="cursor-pointer font-medium text-gray-700">View raw OCR text</summary>
                <pre className="whitespace-pre-wrap bg-gray-100 rounded-lg p-3 mt-2">{receipt.rawOcrText}</pre>
              </details>
            )}
          </div>

          <div className="card-footer justify-between">
            <button type="button" className="btn btn-outline btn-danger" onClick={handleDelete} disabled={deleting}>
              <i className="ki-filled ki-trash" />
              {deleting ? "Deleting…" : "Delete"}
            </button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              <i className="ki-filled ki-check" />
              {saving ? "Saving…" : "Save Changes"}
            </button>
          </div>
        </form>
      </div>
    </>
  );
}

function Confidence({ value }: { value: number | null }) {
  if (value === null) return null;

  return <span className="badge badge-xs badge-outline badge-success">{Math.round(value * 100)}% confidence</span>;
}
