export type Location = {
	id: string;
	name: string;
	timezone: string;
	isActive: boolean;
	address: AddressDto;
	createdAt: string;
	updatedAt: string;
};

export type AddressDto = {
	city: string;
	country: string;
	street: string;
	house: string;
};

export type ActiveFilter = "active" | "inactive" | "all";

export type SortByFilter = "name" | "created";

export type SortDirectionFilter = "asc" | "desc";
