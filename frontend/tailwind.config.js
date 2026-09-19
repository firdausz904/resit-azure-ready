/** Metronic colours are CSS variables (see src/styles/metronic.css), so utilities follow the theme. */
const tone = (name) => ({
  DEFAULT: `var(--tw-${name})`,
  active: `var(--tw-${name}-active)`,
  light: `var(--tw-${name}-light)`,
  clarity: `var(--tw-${name}-clarity)`,
  inverse: `var(--tw-${name}-inverse)`
});

/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  darkMode: ["selector", ".dark"],
  corePlugins: { preflight: false },
  theme: {
    extend: {
      colors: {
        primary: tone("primary"),
        success: tone("success"),
        danger: tone("danger"),
        warning: tone("warning"),
        info: tone("info"),
        dark: tone("dark"),
        light: tone("light"),
        secondary: tone("secondary"),
        gray: {
          100: "var(--tw-gray-100)",
          200: "var(--tw-gray-200)",
          300: "var(--tw-gray-300)",
          400: "var(--tw-gray-400)",
          500: "var(--tw-gray-500)",
          600: "var(--tw-gray-600)",
          700: "var(--tw-gray-700)",
          800: "var(--tw-gray-800)",
          900: "var(--tw-gray-900)"
        }
      },
      fontSize: {
        "1.5xl": ["1.375rem", "1.8125rem"],
        "2sm": ["0.8125rem", "1.125rem"],
        "2xs": ["0.6875rem", "0.75rem"],
        "3xs": ["0.625rem", "0.75rem"]
      },
      boxShadow: {
        primary: "var(--tw-primary-box-shadow)"
      },
      fontFamily: {
        sans: ["Inter", "system-ui", "sans-serif"]
      }
    }
  }
};
