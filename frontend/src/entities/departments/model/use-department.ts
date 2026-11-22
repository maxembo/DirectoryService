import { useQuery } from "@tanstack/react-query";
import { departmentsApi } from "../api/api";
import type { DepartmentId } from "./types";

export function useDepartment(id: DepartmentId) {
	const query = useQuery({
		...departmentsApi.getDepartmentQueryOptions(id),
	});

	return {
		department: query.data?.result,
		isLoading: query.isLoading,
		isError: query.isError,
		error: query.error,
		refetch: query.refetch,
	};
}
