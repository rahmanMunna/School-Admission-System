import { NavLink, Route, Routes } from "react-router-dom";

// Placeholders. We build the real pages in the next steps.
function ClassListPage() { return <h2>Classes</h2>; }
function MyHoldsPage()   { return <h2>My applications</h2>; }
function HoldPage()      { return <h2>Application</h2>; }
function AdminPage()     { return <h2>School admin</h2>; }

export default function App() {
  return (
    <div className="container">
      <header className="header">
        <h1>School Admission</h1>
        <nav>
          <NavLink to="/" end>Classes</NavLink>
          <NavLink to="/my">My applications</NavLink>
          <NavLink to="/admin">School admin</NavLink>
        </nav>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<ClassListPage />} />
          <Route path="/my" element={<MyHoldsPage />} />
          <Route path="/holds/:holdId" element={<HoldPage />} />
          <Route path="/admin" element={<AdminPage />} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>
    </div>
  );
}