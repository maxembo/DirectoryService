export type GlobalSearchId = string;

export type GlobalSearchDto = {
	id: GlobalSearchId;
	type: GlobalSearchType;
	title: string;
	subTitle: string;
	isActive: boolean;
	deletedAt: string | null;
};

export type GlobalSearchType = "department" | "location" | "position";
