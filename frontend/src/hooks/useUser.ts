import { useQuery } from "@tanstack/react-query";
import { useClient } from "./useClient";
import { getCurrentShapeShapeCurrentGet, getMeUserMeGet } from "../client";

export function useUser() {
    // Get the client
    const { client } = useClient();
    
    // Get user data
    const { data: user, status: userStatus } = useQuery({
        queryKey: ['user-data'],
        queryFn: async () => {
            const req = await getMeUserMeGet({client: client});
            return req.data;
        },

        meta: { errorMessage: 'Error al cargar los datos' },
        staleTime: 1000 * 60 * 5,
    })
    
    const verified = userStatus === 'success' ? user.verified : false;
    
    // Get latest shape
    const { data: latestShape, status: latestShapeStatus } = useQuery({
        queryKey: ['latest-shape'],
        queryFn: async () => {
            const req = await getCurrentShapeShapeCurrentGet({client: client});
            return req.data;
        },
        meta: { errorMessage: 'Error al cargar la forma actual' },
        staleTime: 1000 * 60 * 5,
        enabled: verified
    });

    const hasShape = latestShapeStatus === 'success' && latestShape !== null;

    return {
        verified,
        userStatus,
        user,
        latestShape,
        latestShapeStatus,
        hasShape
    }
}