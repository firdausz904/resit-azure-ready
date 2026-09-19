import { useEffect, useRef, useState } from "react";
import { Link, NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import { clearSession, getSession } from "../api/session";

const NAV_ITEMS = [
  { to: "/", label: "Home", icon: "ki-chart-line-star" },
  { to: "/upload", label: "Upload", icon: "ki-cloud-add" },
  { to: "/receipts", label: "Receipts", icon: "ki-bill" },
  { to: "/bills", label: "Bills", icon: "ki-calendar-tick" }
] as const;

const ROUTE_TITLES: Record<string, string> = {
  "/": "Dashboard",
  "/upload": "Upload Receipts",
  "/receipts": "Receipts",
  "/bills": "Bills & Loans"
};

function initialsFrom(name?: string): string {
  const parts = name?.trim().split(/\s+/).filter(Boolean) ?? [];
  if (parts.length === 0) return "?";
  return parts.slice(0, 2).map((part) => part[0].toUpperCase()).join("");
}

function Logo({ size }: { size: "sm" | "lg" }) {
  const box = size === "lg" ? "size-11 text-2xl" : "size-8 text-xl";
  return (
    <Link to="/" className="flex items-center gap-2.5" aria-label="Resit home">
      <span className={`${box} rounded-xl bg-primary text-primary-inverse font-bold flex items-center justify-center shadow-primary`}>
        R
      </span>
    </Link>
  );
}

function Breadcrumb() {
  const { pathname } = useLocation();
  const isDetail = pathname.startsWith("/receipts/");
  const title = isDetail ? "Receipt Details" : ROUTE_TITLES[pathname] ?? "Resit";

  return (
    <div className="flex items-center flex-wrap gap-1.5 text-sm">
      <Link to="/" className="text-gray-700 hover:text-primary">
        Home
      </Link>
      {pathname !== "/" && (
        <>
          <i className="ki-filled ki-right text-3xs text-gray-500" />
          {isDetail && (
            <>
              <Link to="/receipts" className="text-gray-700 hover:text-primary">
                Receipts
              </Link>
              <i className="ki-filled ki-right text-3xs text-gray-500" />
            </>
          )}
          <span className="text-gray-900 font-medium">{title}</span>
        </>
      )}
    </div>
  );
}

function AccountMenu() {
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const session = getSession();
  const displayName = session?.displayName || "Account";
  const initials = session?.initials || initialsFrom(session?.displayName);
  const householdName = session?.householdName;

  useEffect(() => {
    if (!open) return;

    function handleClick(event: MouseEvent) {
      if (ref.current && !ref.current.contains(event.target as Node)) {
        setOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, [open]);

  function handleLogout() {
    clearSession();
    navigate("/login", { replace: true });
  }

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        onClick={() => setOpen((current) => !current)}
        aria-expanded={open}
        aria-label="Account menu"
        className="size-10 rounded-full bg-primary-light text-primary border border-primary-clarity font-semibold text-sm flex items-center justify-center hover:bg-primary hover:text-primary-inverse transition-colors"
      >
        {initials}
      </button>

      {open && (
        <div className="absolute bottom-0 start-full ms-3 z-30 w-[240px] rounded-xl border border-gray-300 bg-light shadow-lg">
          <div className="flex items-center gap-3 px-4 py-3.5 border-b border-gray-200">
            <span className="size-9 rounded-full bg-primary text-primary-inverse font-semibold text-xs flex items-center justify-center shrink-0">
              {initials}
            </span>
            <div className="min-w-0">
              <div className="text-sm font-semibold text-gray-900 truncate">{displayName}</div>
              {householdName && <div className="text-xs text-gray-600 truncate">{householdName}</div>}
            </div>
          </div>
          <div className="p-2">
            <button type="button" onClick={handleLogout} className="btn btn-sm btn-light w-full justify-center">
              <i className="ki-filled ki-exit-right" />
              Log out
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export function AppLayout() {
  const { pathname } = useLocation();
  const [drawerOpen, setDrawerOpen] = useState(false);

  useEffect(() => {
    setDrawerOpen(false);
  }, [pathname]);

  return (
    <div className="flex flex-col grow">
      <header className="flex lg:hidden items-center fixed z-10 top-0 start-0 end-0 h-[var(--tw-header-height)] bg-[var(--tw-page-bg)]">
        <div className="container-fixed flex items-center justify-between">
          <Logo size="sm" />
          <button
            type="button"
            className="btn btn-icon btn-light btn-clear btn-sm -me-1"
            onClick={() => setDrawerOpen(true)}
            aria-label="Open menu"
          >
            <i className="ki-filled ki-menu" />
          </button>
        </div>
      </header>

      {drawerOpen && (
        <div className="fixed inset-0 z-10 bg-black/30 lg:hidden" onClick={() => setDrawerOpen(false)} aria-hidden="true" />
      )}

      <div className="flex grow flex-col pt-[var(--tw-header-height)] lg:pt-0">
        <aside
          className={`fixed top-0 bottom-0 z-20 flex flex-col items-stretch w-[var(--tw-sidebar-width)] bg-[var(--tw-page-bg)] border-e border-gray-200 lg:border-e-0 transition-transform duration-200 lg:translate-x-0 ${
            drawerOpen ? "translate-x-0" : "-translate-x-full"
          }`}
        >
          <div className="hidden lg:flex items-center justify-center shrink-0 pt-8 pb-3.5">
            <Logo size="lg" />
          </div>

          <nav className="grow flex flex-col items-center gap-2.5 pt-8 lg:pt-3" aria-label="Main">
            {NAV_ITEMS.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === "/"}
                className={({ isActive }) =>
                  `w-[62px] h-[60px] flex flex-col justify-center items-center gap-1 p-2 rounded-[9px] border transition-colors hover:bg-light hover:border-gray-200 hover:text-primary ${
                    isActive ? "border-gray-200 bg-light text-primary" : "border-transparent text-gray-600"
                  }`
                }
              >
                <i className={`ki-filled ${item.icon} text-1.5xl`} />
                <span className="text-xs font-medium">{item.label}</span>
              </NavLink>
            ))}
          </nav>

          <div className="flex items-center justify-center shrink-0 pt-3 pb-6">
            <AccountMenu />
          </div>
        </aside>

        <main
          className="flex flex-col grow rounded-xl bg-light border border-gray-300 lg:ms-[var(--tw-sidebar-width)] pt-5 mt-0 lg:mt-5 m-5"
          role="main"
        >
          <div className="pb-5">
            <div className="container-fixed flex items-center justify-between flex-wrap gap-3">
              <Breadcrumb />
            </div>
          </div>
          <div className="container-fixed pb-8">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
