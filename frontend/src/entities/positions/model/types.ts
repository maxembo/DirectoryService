type PositionId = string;

export type PositionDto = {
	id: PositionId;
	name: string;
	description: string | null;
	isActive: boolean;
	createdAt: string;
	updatedAt: string;
	deletedAt: string | null;
};

export type PositionSortByFilter =
	| "name"
	| "created"
	| "updated"
	| "status"
	| "department_count";
