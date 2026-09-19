import { ChangeEvent, DragEvent, useCallback, useEffect, useRef, useState } from "react";
import { httpClient } from "../../api/httpClient";
import { ensureConnected } from "../../api/signalrClient";
import { requireHouseholdId, getSession } from "../../api/session";
import { PageHeader } from "../../components/PageHeader";
import { BatchProgressUpdate, ReceiptProcessedUpdate } from "../../types";
import { formatCurrency } from "../../format";

const MAX_FILES = 100;

interface FileRow {
  fileName: string;
  status: "queued" | "extracting" | "done" | "failed";
  merchant?: string;
  total?: number;
  failureReason?: string;
}

export function UploadPage() {
  const [rows, setRows] = useState<FileRow[]>([]);
  const [progress, setProgress] = useState<BatchProgressUpdate | null>(null);
  const [dragActive, setDragActive] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    let disposed = false;

    ensureConnected().then((hub) => {
      if (disposed) return;

      hub.on("BatchProgressUpdated", (update: BatchProgressUpdate) => setProgress(update));

      hub.on("ReceiptProcessed", (update: ReceiptProcessedUpdate) => {
        setRows((current) =>
          current.map((row) =>
            row.fileName === update.fileName
              ? {
                  fileName: row.fileName,
                  status: update.status === "Failed" ? "failed" : "done",
                  merchant: update.merchant ?? undefined,
                  total: update.total ?? undefined,
                  failureReason: update.failureReason ?? undefined
                }
              : row
          )
        );
      });
    });

    return () => {
      disposed = true;
    };
  }, []);

  const uploadFiles = useCallback(async (files: FileList) => {
    setError(null);

    if (files.length === 0) {
      return;
    }

    if (files.length > MAX_FILES) {
      setError(`You can upload up to ${MAX_FILES} files at once.`);
      return;
    }

    const householdId = requireHouseholdId();
    const userId = getSession()?.userId ?? "";

    setRows(Array.from(files).map((file) => ({ fileName: file.name, status: "queued" })));
    setProgress({ batchId: "", totalCount: files.length, processedCount: 0, failedCount: 0 });

    const formData = new FormData();
    Array.from(files).forEach((file) => formData.append("files", file));

    setRows((current) => current.map((row) => ({ ...row, status: "extracting" })));

    try {
      await httpClient.post(
        `/households/${householdId}/receipts/batches?uploadedByUserId=${userId}`,
        formData
      );
    } catch {
      setError("Couldn't start the upload. Please try again.");
    }
  }, []);

  function handleInputChange(event: ChangeEvent<HTMLInputElement>) {
    if (event.target.files) {
      uploadFiles(event.target.files);
    }
  }

  function handleDrop(event: DragEvent<HTMLDivElement>) {
    event.preventDefault();
    setDragActive(false);

    if (event.dataTransfer.files) {
      uploadFiles(event.dataTransfer.files);
    }
  }

  const percentage = progress && progress.totalCount > 0
    ? Math.round((progress.processedCount / progress.totalCount) * 100)
    : 0;

  return (
    <>
      <PageHeader
        title="Upload Receipts"
        description={`Snap a photo or upload up to ${MAX_FILES} images and PDFs at once.`}
      />

      <div
        className={`flex flex-col items-center justify-center gap-3 text-center rounded-xl border-2 border-dashed p-10 mb-5 transition-colors ${
          dragActive ? "border-primary bg-primary-light" : "border-gray-300 bg-gray-100"
        }`}
        onDragOver={(event) => {
          event.preventDefault();
          setDragActive(true);
        }}
        onDragLeave={() => setDragActive(false)}
        onDrop={handleDrop}
      >
        <span className="size-14 rounded-2xl bg-primary-light text-primary flex items-center justify-center" aria-hidden="true">
          <i className="ki-filled ki-cloud-add text-3xl" />
        </span>
        <div className="text-base font-semibold text-gray-900">Drag &amp; drop receipts here</div>
        <div className="text-2sm text-gray-600">JPG, PNG or PDF — up to {MAX_FILES} files, 10 MB each</div>
        <button type="button" className="btn btn-primary mt-1" onClick={() => inputRef.current?.click()}>
          <i className="ki-filled ki-folder-up" />
          Browse Files
        </button>
        <input
          ref={inputRef}
          type="file"
          multiple
          accept="image/jpeg,image/png,application/pdf"
          hidden
          onChange={handleInputChange}
        />
      </div>

      {error && (
        <div className="flex items-center gap-2 rounded-md bg-danger-light text-danger text-2sm font-medium px-4 py-3 mb-5">
          <i className="ki-filled ki-information-2" />
          {error}
        </div>
      )}

      {rows.length > 0 && (
        <div className="card">
          <div className="card-header">
            <div>
              <h3 className="card-title">
                Processing batch — {progress?.processedCount ?? 0} of {progress?.totalCount ?? rows.length}
              </h3>
              <div className="text-xs text-gray-600">Live updates via SignalR</div>
            </div>
            <div className="text-xl font-semibold text-primary">{percentage}%</div>
          </div>

          <div className="card-body">
            <div className="progress progress-primary h-2 mb-4">
              <div className="progress-bar bg-primary transition-all duration-300" style={{ width: `${percentage}%` }} />
            </div>

            <ul className="max-h-[360px] overflow-y-auto">
              {rows.map((row) => (
                <li key={row.fileName} className="flex items-center gap-3.5 py-3 border-b border-gray-200 last:border-b-0">
                  <span className="size-10 rounded-lg bg-gray-100 text-gray-600 flex items-center justify-center shrink-0" aria-hidden="true">
                    <i className="ki-filled ki-file-added text-xl" />
                  </span>
                  <div className="grow min-w-0">
                    <div className="text-sm font-medium text-gray-900 truncate">{row.fileName}</div>
                    {row.status === "done" && (
                      <div className="text-xs text-success">
                        {row.merchant} · {formatCurrency(row.total ?? 0)}
                      </div>
                    )}
                    {row.status === "failed" && <div className="text-xs text-danger">{row.failureReason}</div>}
                    {row.status === "extracting" && (
                      <div className="text-xs text-gray-600">Extracting merchant, total and date…</div>
                    )}
                    {row.status === "queued" && <div className="text-xs text-gray-600">Waiting in queue…</div>}
                  </div>
                  <span className={`badge badge-sm badge-outline ${STATUS_BADGE[row.status]}`}>{statusLabel(row.status)}</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}
    </>
  );
}

const STATUS_BADGE: Record<FileRow["status"], string> = {
  done: "badge-success",
  failed: "badge-danger",
  extracting: "badge-primary",
  queued: ""
};

function statusLabel(status: FileRow["status"]): string {
  switch (status) {
    case "done":
      return "Done";
    case "failed":
      return "Failed";
    case "extracting":
      return "Extracting";
    default:
      return "Queued";
  }
}
