import { departmentsApi } from "@/entities/departments/api/api";
import { locationsApi } from "@/entities/locations/api/api";
import { positionsApi } from "@/entities/positions/api/api";
import { resetDepartmentTreeData } from "@/features/departments/department-tree/model/department-tree-store";
import { EnvelopeError } from "@/shared/api/errors";
import { queryClient } from "@/shared/api/query-client";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRestoreDepartment() {
	const mutation = useMutation({
		mutationFn: departmentsApi.restoreDepartment,
		onSuccess: async () => {
			resetDepartmentTreeData();

			await Promise.all([
				queryClient.invalidateQueries({
					queryKey: [departmentsApi.baseKey],
				}),
				queryClient.invalidateQueries({
					queryKey: [locationsApi.baseKey],
				}),
				queryClient.invalidateQueries({
					queryKey: [positionsApi.baseKey],
				}),
			]);

			toast.success(`Подразделение успешно восстановлено`);
		},
		onError: (error) => {
			if (error instanceof EnvelopeError) {
				toast.error(error.message);
			} else {
				toast.error("Произошла ошибка при восстановлении подразделения");
			}
		},
	});

	return {
		restoreDepartment: mutation.mutateAsync,
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
	};
}
