// Shows any error in a red box. Shows nothing when there's no error.
export default function ErrorMessage({ error }) {
  if (!error) return null;
  return <div className="error">{error.message ?? String(error)}</div>;
}