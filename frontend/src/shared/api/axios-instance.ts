import axios from "axios";
import { Envelope } from "./envelops";
import { EnvelopeError } from "./errors";

const HEADERS = { "Content-Type": "application/json" };

export const apiClient = axios.create({
	baseURL: process.env.NEXT_PUBLIC_API_URL,
	headers: HEADERS,
	paramsSerializer: {
		indexes: null,
	},
});

apiClient.interceptors.response.use(
	(response) => {
		const envelope = response.data as Envelope;

		if (envelope.isError && envelope.errorsList) {
			throw new EnvelopeError(envelope.errorsList);
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
