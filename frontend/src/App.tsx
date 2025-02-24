import { Route, Routes } from "react-router-dom"

import Login from "./pages/login"
import Media from "./pages/media" 
import MediaDebug from "./pages/mediadebug"

function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/media" element={<Media />} />
      <Route path="/mediadebug" element={<MediaDebug />} />
    </Routes>
  )
}

export default App

