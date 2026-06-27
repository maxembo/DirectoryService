export type LocationId = string;

export type Location = {
	id: LocationId;
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

export type LocationSortByFilter = "name" | "created";
