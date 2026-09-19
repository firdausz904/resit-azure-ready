import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { httpClient } from "../../api/httpClient";
import { requireHouseholdId } from "../../api/session";
import { PageHeader } from "../../components/PageHeader";
import { ReceiptSummary } from "../../types";
import { formatCurrency, formatDate } from "../../format";

const PAGE_SIZE = 10;

export function ReceiptsPage() {
  const [receipts, setReceipts] = useState<ReceiptSummary[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [reloadKey, setReloadKey] = useState(0);
  const [selected, setSelected] = useState<Set<string>>(new Set());

  const allSelected = receipts.length > 0 && receipts.every((receipt) => selected.has(receipt.id));

  function toggleOne(id: string) {
    setSelected((current) => {
      const next = new Set(current);
      if (!next.delete(id)) next.add(id);
      return next;
    });
  }

  function toggleAll() {
    setSelected(allSelected ? new Set() : new Set(receipts.map((receipt) => receipt.id)));
  }

  async function handleBulkDelete() {
    const count = selected.size;
    if (!window.confirm(`Delete ${count} receipt${count > 1 ? "s" : ""}? This cannot be undone.`)) return;

    await httpClient.post(`/households/${requireHouseholdId()}/receipts/bulk-delete`, { ids: [...selected] });

    if (allSelected && page > 1) {
      setPage((current) => current - 1);
    } else {
      setReloadKey((current) => current + 1);
    }
  }

  async function handleDelete(receipt: ReceiptSummary) {
    if (!window.confirm(`Delete receipt from "${receipt.merchant}"? This cannot be undone.`)) return;

    await httpClient.delete(`/households/${requireHouseholdId()}/receipts/${receipt.id}`);

    if (receipts.length === 1 && page > 1) {
      setPage((current) => current - 1);
    } else {
      setReloadKey((current) => current + 1);
    }
  }

  useEffect(() => {
    const householdId = requireHouseholdId();
    const params = new URLSearchParams({
      page: String(page),
      pageSize: String(PAGE_SIZE),
      ...(search ? { search } : {})
    });

    httpClient
      .get<{ items: ReceiptSummary[]; totalCount: number }>(`/households/${householdId}/receipts?${params}`)
      .then((result) => {
        setReceipts(result.items);
        setTotalCount(result.totalCount);
        setSelected(new Set());
      })
      .catch(() => {
        setReceipts([]);
        setTotalCount(0);
        setSelected(new Set());
      });
  }, [page, search, reloadKey]);

  const from = (page - 1) * PAGE_SIZE + 1;
  const to = Math.min(page * PAGE_SIZE, totalCount);

  return (
    <>
      <PageHeader
        title="All Receipts"
        description="Every receipt your household has uploaded."
        actions={
          <Link to="/upload" className="btn btn-primary">
            <i className="ki-filled ki-cloud-add" />
            Upload Receipts
          </Link>
        }
      />

      <div className="card card-grid min-w-full">
        <div className="card-header flex-wrap gap-2">
          <h3 className="card-title font-medium text-sm">
            {totalCount === 0 ? "No receipts" : `Showing ${from}–${to} of ${totalCount}`}
          </h3>
          {selected.size > 0 && (
            <button type="button" className="btn btn-sm btn-outline btn-danger" onClick={handleBulkDelete}>
              <i className="ki-filled ki-trash" />
              Delete selected ({selected.size})
            </button>
          )}
          <label className="input input-sm w-full sm:w-64">
            <i className="ki-filled ki-magnifier" />
            <input
              type="search"
              placeholder="Search receipts"
              value={search}
              onChange={(event) => {
                setPage(1);
                setSearch(event.target.value);
              }}
            />
          </label>
        </div>

        <div className="card-body">
          <div className="scrollable-x-auto">
            <table className="table table-auto table-border">
              <thead>
                <tr>
                  <th className="w-[40px]">
                    <input
                      type="checkbox"
                      className="checkbox checkbox-sm"
                      checked={allSelected}
                      onChange={toggleAll}
                      disabled={receipts.length === 0}
                      aria-label="Select all receipts on this page"
                    />
                  </th>
                  <th className="min-w-[220px] font-normal text-gray-700">Receipt</th>
                  <th className="min-w-[130px] font-normal text-gray-700">Date</th>
                  <th className="min-w-[140px] font-normal text-gray-700">Category</th>
                  <th className="min-w-[120px] text-end font-normal text-gray-700">Amount</th>
                  <th className="min-w-[130px] text-center font-normal text-gray-700">Status</th>
                  <th className="w-[60px]" aria-label="Actions" />
                </tr>
              </thead>
              <tbody>
                {receipts.map((receipt) => (
                  <tr key={receipt.id}>
                    <td>
                      <input
                        type="checkbox"
                        className="checkbox checkbox-sm"
                        checked={selected.has(receipt.id)}
                        onChange={() => toggleOne(receipt.id)}
                        aria-label={`Select receipt from ${receipt.merchant}`}
                      />
                    </td>
                    <td>
                      <Link to={`/receipts/${receipt.id}`} className="flex items-center gap-3 group">
                        <span className="size-9 rounded-lg bg-primary-light text-primary flex items-center justify-center shrink-0">
                          <i className="ki-filled ki-bill text-lg" />
                        </span>
                        <span className="text-sm font-medium text-gray-900 group-hover:text-primary">
                          {receipt.merchant}
                        </span>
                      </Link>
                    </td>
                    <td className="text-gray-700">{formatDate(receipt.purchasedOn)}</td>
                    <td>
                      <span className="badge badge-sm badge-outline badge-primary">{receipt.category}</span>
                    </td>
                    <td className="text-end font-semibold text-gray-900">{formatCurrency(receipt.total)}</td>
                    <td className="text-center">
                      <span className={`badge badge-sm badge-outline ${statusBadge(receipt.status)}`}>
                        {receipt.status === "NeedsReview" ? "Needs Review" : receipt.status}
                      </span>
                    </td>
                    <td className="text-end">
                      <button
                        type="button"
                        className="btn btn-sm btn-icon btn-light btn-clear text-danger"
                        onClick={() => handleDelete(receipt)}
                        aria-label={`Delete receipt from ${receipt.merchant}`}
                      >
                        <i className="ki-filled ki-trash" />
                      </button>
                    </td>
                  </tr>
                ))}
                {receipts.length === 0 && (
                  <tr>
                    <td colSpan={7} className="text-center text-gray-600 py-10">
                      <i className="ki-filled ki-bill text-3xl text-gray-500 block mb-2" />
                      No receipts match your filters.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        <div className="card-footer justify-end gap-3 text-gray-600 text-2sm font-medium">
          <span>
            Page {page}
          </span>
          <div className="pagination">
            <button
              type="button"
              className="btn btn-icon btn-sm btn-light"
              disabled={page === 1}
              onClick={() => setPage((current) => current - 1)}
              aria-label="Previous page"
            >
              <i className="ki-filled ki-left" />
            </button>
            <button
              type="button"
              className="btn btn-icon btn-sm btn-light"
              disabled={to >= totalCount}
              onClick={() => setPage((current) => current + 1)}
              aria-label="Next page"
            >
              <i className="ki-filled ki-right" />
            </button>
          </div>
        </div>
      </div>
    </>
  );
}

function statusBadge(status: string): string {
  switch (status.toLowerCase()) {
    case "done":
      return "badge-success";
    case "failed":
      return "badge-danger";
    case "needsreview":
      return "badge-warning";
    case "extracting":
      return "badge-primary";
    default:
      return "";
  }
}
