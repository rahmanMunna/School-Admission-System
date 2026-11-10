import { useState } from "react";
import { NavLink, Route, Routes, useLocation, useNavigate } from "react-router-dom";
import StudentBar from "./components/StudentBar";
import { clearStudentId, loadStudentId, saveStudentId } from "./studentId";
import ClassListPage from "./pages/ClassListPage";
import HoldPage from "./pages/HoldPage";
import MyHoldsPage from "./pages/MyHoldsPage";
import AdminPage from "./pages/AdminPage";

export default function App() {
  // Read the saved ID once when the app starts (survives refresh / reopening the browser)
  const [studentId, setStudentId] = useState(loadStudentId);
  const location = useLocation();
  const navigate = useNavigate();

  const isAdminPage = location.pathname.startsWith("/admin");

  function changeStudent(id) {
    if (id) saveStudentId(id);
    else clearStudentId();
    setStudentId(id);
    navigate("/");                     // go back to the class list after switching
  }

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

      {/* The admin area doesn't need a student ID */}
      {!isAdminPage && <StudentBar studentId={studentId} onChange={changeStudent} />}

      <main>
        <Routes>
          <Route path="/" element={<ClassListPage studentId={studentId} />} />
          <Route path="/my" element={<MyHoldsPage studentId={studentId} />} />
          <Route path="/holds/:holdId" element={<HoldPage studentId={studentId} />} />
          <Route path="/admin" element={<AdminPage />} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>
    </div>
  );
}