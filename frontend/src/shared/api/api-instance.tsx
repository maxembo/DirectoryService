import axios from "axios";

export const apiClient = axios.create({
	baseURL: "http://localhost:5051/api",
	headers: { "Content-Type": "application/json" },
})