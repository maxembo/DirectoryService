import axios from "axios";
import { Envelope } from "./envelops";
import { EnvelopeError } from "./errors";

const API_BASE_URL = "http://localhost:5051/api";
const HEADERS = { "Content-Type": "application/json" };

export const apiClient = axios.create({
	baseURL: API_BASE_URL,
	headers: HEADERS,
});

apiClient.interceptors.response.use(
	(response) => {
		const data = response.data as Envelope;

		if (data.isError && data.errorsList) {
		}

		return response;
	},
	(error) => {
		if (axios.isAxiosError(error) && error.response?.data) {
			const envelope = error.response.data as Envelope;

			if (envelope.isError && envelope.errorsList) {
				throw new EnvelopeError(envelope.errorsList);
			}
		}

		return Promise.reject(error);
	},
);
