export interface ReceiptSummary {
  id: string;
  merchant: string;
  total: number;
  purchasedOn: string | null;
  category: string;
  status: "Queued" | "Extracting" | "NeedsReview" | "Done" | "Failed";
  uploadedByUserId: string;
}

export interface BatchProgressUpdate {
  batchId: string;
  totalCount: number;
  processedCount: number;
  failedCount: number;
}

export interface ReceiptProcessedUpdate {
  batchId: string;
  receiptId: string;
  fileName: string;
  status: string;
  merchant: string | null;
  total: number | null;
  failureReason: string | null;
}

export interface ReminderDueUpdate {
  billId: string;
  name: string;
  amount: number;
  dueDate: string;
  daysUntilDue: number;
}

export interface UpcomingBill {
  id: string;
  name: string;
  amount: number;
  dueDate: string;
  recurrence: "OneTime" | "Weekly" | "Monthly" | "Quarterly" | "Yearly";
  category: string;
  isPaid: boolean;
  daysUntilDue: number;
  notifyDaysBefore: number;
}

export interface DashboardSummary {
  totalSpent: number;
  receiptCount: number;
  averagePerReceipt: number;
  spendByCategory: Record<string, number>;
}
