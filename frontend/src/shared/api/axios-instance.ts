import axios from "axios";
import { Envelope } from "./envelops";
import { EnvelopeError } from "./errors";

export const apiClient = axios.create({
	baseURL: "http://localhost:5051/api",
	headers: { "Content-Type": "application/json" },
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
