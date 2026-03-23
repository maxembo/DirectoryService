export type Location = {
	id: string;
	name: string;
	timezone: string;
	isActive: boolean;
	address: AddressDto;
	updatedAt: string;
};

export type AddressDto = {
	city: string;
	country: string;
	street: string;
	house: string;
};
