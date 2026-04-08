import * as z from "zod";

const MIN_LOCATION_TEXT_LENGTH = 1;
const MIN_LOCATION_NAME_LENGTH = 3;

const MAX_LOCATION_ADDRESS_LENGTH = 100;
const MAX_LOCATION_NAME_LENGTH = 120;

const addressSchema = z.object({
	country: z
		.string()
		.trim()
		.min(MIN_LOCATION_TEXT_LENGTH, "Страна обязательна")
		.max(
			MAX_LOCATION_ADDRESS_LENGTH,
			`Страна не должна превышать ${MAX_LOCATION_ADDRESS_LENGTH} символов`,
		),
	city: z
		.string()
		.trim()
		.min(MIN_LOCATION_TEXT_LENGTH, "Город обязателен")
		.max(
			MAX_LOCATION_ADDRESS_LENGTH,
			`Город не должен превышать ${MAX_LOCATION_ADDRESS_LENGTH} символов`,
		),
	street: z
		.string()
		.trim()
		.min(MIN_LOCATION_TEXT_LENGTH, "Улица обязательна")
		.max(
			MAX_LOCATION_ADDRESS_LENGTH,
			`Улица не должна превышать ${MAX_LOCATION_ADDRESS_LENGTH} символов`,
		),
	house: z
		.string()
		.trim()
		.min(MIN_LOCATION_TEXT_LENGTH, "Номер дома обязателен")
		.max(
			MAX_LOCATION_ADDRESS_LENGTH,
			`Номер дома не должен превышать ${MAX_LOCATION_ADDRESS_LENGTH} символов`,
		),
});

export const createLocationSchema = z.object({
	name: z
		.string()
		.trim()
		.min(MIN_LOCATION_NAME_LENGTH, "Название должно быть не менее 3 символов")
		.max(
			MAX_LOCATION_NAME_LENGTH,
			`Название не должно превышать ${MAX_LOCATION_NAME_LENGTH} символов`,
		),
	address: addressSchema,
	timezone: z
		.string()
		.trim()
		.min(MIN_LOCATION_TEXT_LENGTH, "Часовой пояс обязателен")
		.max(
			MAX_LOCATION_ADDRESS_LENGTH,
			`Часовой пояс не должен превышать ${MAX_LOCATION_ADDRESS_LENGTH} символов`,
		)
		.regex(
			/^[A-Za-z]+\/[A-Za-z_]+(?:\/[A-Za-z_]+)?$/,
			"Введите часовой пояс в формате Area/City, например Europe/Moscow",
		),
});

export type CreateLocationFormData = z.infer<typeof createLocationSchema>;
