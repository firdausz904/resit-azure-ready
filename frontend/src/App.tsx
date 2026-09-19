import { Navigate, Route, Routes } from "react-router-dom";
import { AppLayout } from "./layout/AppLayout";
import { LoginPage } from "./features/auth/LoginPage";
import { DashboardPage } from "./features/dashboard/DashboardPage";
import { UploadPage } from "./features/upload/UploadPage";
import { ReceiptsPage } from "./features/receipts/ReceiptsPage";
import { ReceiptDetailPage } from "./features/receipts/ReceiptDetailPage";
import { BillsPage } from "./features/bills/BillsPage";
import { getSession } from "./api/session";

function isAuthenticated(): boolean {
  return getSession() !== null;
}

function RequireAuth({ children }: { children: JSX.Element }) {
  return isAuthenticated() ? children : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route path="/" element={<DashboardPage />} />
        <Route path="/upload" element={<UploadPage />} />
        <Route path="/receipts" element={<ReceiptsPage />} />
        <Route path="/receipts/:receiptId" element={<ReceiptDetailPage />} />
        <Route path="/bills" element={<BillsPage />} />
      </Route>
    </Routes>
  );
}
