import { useQuery } from "@tanstack/react-query";
import { useClient } from "./useClient";
import { getCurrentShapeShapeCurrentGet, getMeUserMeGet } from "../client";

export function useUser() {
  // Get the client
  const { client, logout: client_logout } = useClient();

  // Get user data
  const {
    data: user,
    status: userStatus,
    refetch: refetchUser,
  } = useQuery({
    queryKey: ["user-data"],
    queryFn: async () => {
      console.log("Fetching user data");
      const req = await getMeUserMeGet({ client: client });
      console.log("User data fetched", req);
      return req.data;
    },

    // meta: { errorMessage: "Error al cargar los datos" },
    staleTime: 1000 * 60 * 5,
  });

  const verified = userStatus === "success" ? user.verified : false;

  // Get latest shape
  const {
    data: latestShape,
    status: latestShapeStatus,
    refetch: refetchShape,
  } = useQuery({
    queryKey: ["latest-shape"],
    queryFn: async () => {
      const req = await getCurrentShapeShapeCurrentGet({
        client: client,
        throwOnError: false,
      });
      if (req.response.status === 404) {
        return null;
      } else if (req.response.status !== 200) {
        throw new Error("Error al cargar la forma actual");
      }
      return req.data;
    },
    meta: { errorMessage: "Error al cargar la forma actual" },
    staleTime: 1000 * 60 * 5,
    enabled: verified,
  });

  const hasShape = latestShapeStatus === "success" && latestShape !== null;
  const refetch = () => {
    const actuallyRefetch = () => {
      refetchUser();
      refetchShape();
    };

    if (userStatus !== "success") {
      setTimeout(actuallyRefetch, 500);
    } else {
      actuallyRefetch();
    }
  };

  const logout = () => {
    client_logout();
    refetch();
  };

  return {
    verified,
    userStatus,
    user,
    latestShape,
    latestShapeStatus,
    hasShape,
    refetch,
    logout,
  };
}
