import {
	departmentsApi,
	optimisticallyUpdateDepartmentActivity,
	restoreDepartmentQueries,
	type ChangeDepartmentActivityRequest,
} from "@/entities/departments";
import { EnvelopeError } from "@/shared/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useToggleDepartmentActivity() {
	const queryClient = useQueryClient();

	const mutation = useMutation({
		mutationFn: departmentsApi.changeDepartmentActivity,
		onMutate: async (request: ChangeDepartmentActivityRequest) => {
			await queryClient.cancelQueries({
				queryKey: [departmentsApi.baseKey],
			});

			const snapshots = optimisticallyUpdateDepartmentActivity(
				queryClient,
				request,
			);

			return { snapshots };
		},
		onError: (error, _request, context) => {
			if (context) {
				restoreDepartmentQueries(queryClient, context.snapshots);
			}

			if (error instanceof EnvelopeError) {
				toast.error(error.allMessages);
				return;
			}
			toast.error("Ошибка при изменении статуса активности");
		},
		onSettled: () =>
			queryClient.invalidateQueries({
				queryKey: [departmentsApi.baseKey],
			}),
	});

	return {
		toggleDepartmentActivity: mutation.mutate,
		isPending: mutation.isPending,
	};
}
