/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        brand: "#6552D0",
        surface: "#A5A5A5",
        background: "#F8F9FC",
        border: "#E5E7EB",
        text: {
          primary: "#111827",
          secondary: "#6B7280",
          muted: "#9CA3AF",
        },
      },

      fontSize: {
        h1: ["28px", "36px"],   
        h2: ["22px", "30px"],   
        h3: ["18px", "26px"],  
        h4: ["16px", "24px"],   
        body: ["14px", "22px"], 
        small: ["12px", "18px"] 
      },

      spacing: {
        xs: "4px",
        sm: "8px",
        md: "16px",
        lg: "24px",
        xl: "32px",
        "2xl": "40px",
        section: "48px", 
      },

      borderRadius: {
        sm: "6px",
        md: "10px",
        lg: "14px",
        xl: "18px",
      },

      fontFamily: {
        primary: ["Inter", "sans-serif"],
      },

      boxShadow: {
        card: "0 4px 12px rgba(0,0,0,0.08)",
        soft: "0 2px 6px rgba(0,0,0,0.06)",
      },
    },
  },
  plugins: [],
};
