import { Route, Routes } from "react-router-dom"
import { PrivateRoutes } from "./components/PrivateRoutes"

import Login from "./pages/login"
import Media from "./pages/media/media" 
import MediaDebug from "./pages/media/mediadebug"
import My from "./pages/my/my"

function App() {
  return (
    <Routes>
      <Route element={<PrivateRoutes login={false} route={"/my/data"}/>}>
        <Route path="/login" element={<Login />} />
      </Route>
      <Route path="/media" element={<Media />} />
      <Route path="/mediadebug" element={<MediaDebug />} />
      <Route element={<PrivateRoutes login={true} route={"/login"} />}>
        <Route path="/my/*" element={<My />} />
      </Route>
    </Routes>
  )
}

export default App

