import { FormEvent, useState } from "react";
import { useNavigate } from "react-router-dom";
import { httpClient } from "../../api/httpClient";
import { saveSession } from "../../api/session";

interface LoginResponse {
  token: string;
  householdId: string;
  userId: string;
  displayName: string;
  initials: string;
  householdName: string;
}

export function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      const session = await httpClient.post<LoginResponse>("/auth/login", { email, password });
      saveSession(session);
      navigate("/");
    } catch {
      setError("Couldn't sign you in. Check your email and password.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="flex flex-col items-center justify-center grow gap-6 p-5 bg-[var(--tw-page-bg)]">
      <div className="flex items-center gap-2.5">
        <span className="size-11 rounded-xl bg-primary text-primary-inverse text-2xl font-bold flex items-center justify-center shadow-primary">
          R
        </span>
        <span className="text-2xl font-semibold text-gray-900">Resit</span>
      </div>

      <div className="card max-w-[370px] w-full">
        <form className="card-body flex flex-col gap-5 p-10" onSubmit={handleSubmit}>
          <div className="text-center mb-2.5">
            <h3 className="text-lg font-medium text-gray-900 leading-none mb-2.5">Welcome back</h3>
            <p className="text-2sm text-gray-700 font-medium">Track every ringgit, one receipt at a time.</p>
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="email" className="form-label font-normal text-gray-900">
              Email
            </label>
            <input
              id="email"
              className="input"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="you@family.com"
              autoComplete="email"
              required
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="password" className="form-label font-normal text-gray-900">
              Password
            </label>
            <div className="input flex items-center gap-2">
              <input
                id="password"
                className="grow bg-transparent outline-none min-w-0"
                type={showPassword ? "text" : "password"}
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Enter password"
                autoComplete="current-password"
                required
              />
              <button
                type="button"
                className="text-gray-500 hover:text-primary"
                onClick={() => setShowPassword((current) => !current)}
                aria-label={showPassword ? "Hide password" : "Show password"}
              >
                <i className={`ki-filled ${showPassword ? "ki-eye-slash" : "ki-eye"}`} />
              </button>
            </div>
          </div>

          {error && (
            <div className="flex items-center gap-2 rounded-md bg-danger-light text-danger text-2sm font-medium px-3 py-2.5">
              <i className="ki-filled ki-information-2" />
              {error}
            </div>
          )}

          <button type="submit" className="btn btn-primary flex justify-center grow" disabled={submitting}>
            {submitting ? "Signing in…" : "Sign In"}
          </button>

          <p className="text-2xs text-gray-600 text-center">Ask a household admin to send you an invite link.</p>
        </form>
      </div>
    </div>
  );
}
