import { useCallback, useEffect, useState } from "react";

// Calls load() right away and then every intervalMs.
// If one call fails, we keep the last good data and just show the error.
export function usePolling(load, intervalMs, deps = []) {
  const [data, setData] = useState(null);
  const [error, setError] = useState(null);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  const refresh = useCallback(async () => {
    try {
      setData(await load());
      setError(null);
    } catch (err) {
      setError(err);
    }
  }, deps);

  useEffect(() => {
    refresh();                                  // first load
    const timer = setInterval(refresh, intervalMs);
    return () => clearInterval(timer);          // stop when leaving the page
  }, [refresh, intervalMs]);

  return { data, error, refresh };
}