import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",               // pretend browser for React components
    setupFiles: "./src/setupTests.js",  // runs before every test file
  },
});