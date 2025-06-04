import { Route, Routes } from "react-router-dom";
import { PrivateRoutes } from "./components/PrivateRoutes";

import My from "./pages/my/my";
import Login from "./pages/login";
import Dashboard from "./pages/dashboard";
import Media from "./components/Media/Media";
import MediaDebug from "./components/Media/MediaDebug";
import { NotFoundPage } from "./components/NotFound/NotFound";

function App() {
  return (
    <Routes>
      <Route path="/" element={<Dashboard />} />
      <Route
        element={<PrivateRoutes requireLogin={false} route={"/my/data"} />}
      >
        <Route path="/login" element={<Login />} />
      </Route>
      <Route path="/media" element={<Media />} />
      <Route path="/mediadebug" element={<MediaDebug />} />
      <Route element={<PrivateRoutes requireLogin={true} route={"/login"} />}>
        <Route path="/my/*" element={<My />} />
      </Route>

      <Route path="*" element={<NotFoundPage />} /> 
    </Routes>
  );
}

export default App;
