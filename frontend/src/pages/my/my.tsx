import { Routes, Route } from "react-router-dom";
import { refresh_token } from "../../utils/session";

export default function My() {
    refresh_token(); 

    return (
        <>
        <div>My</div>
        <Routes>
            <Route index path="data" element={<div>data</div>} />
            <Route path="stats" element={<div>stats</div>} />
        </Routes>
        </>
    );
}
