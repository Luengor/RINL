import {
  Center,
  Grid,
  Group,
  Loader,
  Paper,
  SegmentedControl,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { getActivitiesActivityGet, getShapesShapeGet } from "../../client";
import { ActivityUuid, ShapeFull } from "../../client";
import {
  BarChart,
  BarChartProps,
  DonutChart,
  DonutChartCell,
  LineChart,
  LineChartProps,
} from "@mantine/charts";
import { useEffect, useRef, useState } from "react";
import { useClient } from "../../hooks/useClient";
import useBackground from "../../hooks/useBackground";

interface ApiData {
  activity: ActivityUuid[];
  shape: ShapeFull[];
}

interface PreprocessedData {
  activities: ActivityUuid[];
  shapes: ShapeFull[];
  groupedData: {
    date: string;
    count: number;
    points: number;
    duration: number;
  }[];
  playCount: DonutChartCell[];
  playTime: DonutChartCell[];
}

function Col({ children }: { children: React.ReactNode }) {
  return (
    <Grid.Col span={{ sm: 12, lg: 6 }} px={{ sm: "sm", lg: "md" }}>
      {children}
    </Grid.Col>
  );
}

const defaultLineChartConfig: LineChartProps = {
  data: [],
  series: [],

  h: "25vh",
  dataKey: "date",
  curveType: "monotone",
  tickLine: "none",
  xAxisProps: {
    padding: {
      left: 10,
      right: 20,
    },
  },
};

const Colors = ["blue", "orange", "purple", "cyan", "pink", "gray"];

const defaultBarChartConfig: BarChartProps = {
  data: [],
  series: [],

  h: "25vh",
  dataKey: "date",
  xAxisProps: {
    padding: {
      left: 10,
      right: 20,
    },
  },
};

function Card({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <Col>
      <Paper shadow="md" p="md" style={{ position: "relative" }}>
        <Title order={3} mb="xl" fw="inherit">
          {title}
        </Title>
        {children}
      </Paper>
    </Col>
  );
}

export default function Stats() {
  // Get client
  const { client } = useClient();

  // Get activity and shape data
  const { data, status, refetch } = useQuery({
    queryKey: ["activity-shape-data"],
    queryFn: async () => {
      const activity_data = getActivitiesActivityGet({ client });
      const shape_data = getShapesShapeGet({ client });
      const data = await Promise.all([activity_data, shape_data]);
      return {
        activity: data[0].data,
        shape: data[1].data,
      } as ApiData;
    },
    meta: { errorMessage: "Error al cargar los datos" },
    placeholderData: { activity: [], shape: [] },
    staleTime: 1000 * 60 * 10,
  });

  const has_data = data.activity.length > 0 || data.shape.length > 0;

  const [dataRange, setDataRange] = useState("week");
  const [chartData, setChartData] = useState<PreprocessedData>({
    activities: [],
    shapes: [],
    groupedData: [],
    playCount: [],
    playTime: [],
  });

  const isMobile = window.innerWidth <= window.innerHeight;
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  useBackground({
    canvasRef,
    started: false,
    text: "Estádisticas",
    fontSize: isMobile ? 40 : 80,
    columnSep: isMobile ? 40 : 80,
    scrollSpeed: 0.6,
    randomSpeedMagnitude: 0.1,
  });

  useEffect(() => {
    if (status !== "success") return;

    const now = new Date();
    now.setHours(0, 0, 0, 0);
    const weekAgo = new Date(now.getTime() - 1000 * 60 * 60 * 24 * 7);
    const monthAgo = new Date(now.getTime() - 1000 * 60 * 60 * 24 * 30);
    const yearAgo = new Date(now.getTime() - 1000 * 60 * 60 * 24 * 365);

    const filteredData: PreprocessedData = {
      activities: [],
      shapes: [],
      groupedData: [],
      playCount: [],
      playTime: [],
    };

    const inDateShapes = data.shape.filter((shape) => {
      const date = new Date(shape.date);
      return dataRange === "week"
        ? date >= weekAgo
        : dataRange === "month"
        ? date >= monthAgo
        : dataRange === "year"
        ? date >= yearAgo
        : true;
    });

    // Add at least one shape to the filtered data
    if (inDateShapes.length === 0 && data.shape.length > 0) {
      const lastShape = data.shape[data.shape.length - 1];
      inDateShapes.push(lastShape);
    }

    filteredData.shapes = inDateShapes.map((shape) => {
      const date = new Date(shape.date);
      shape.date = date.toISOString().split("T")[0];
      return shape;
    });

    filteredData.activities = data.activity
      .filter((activity) => {
        const date = new Date(activity.date);
        return dataRange === "week"
          ? date >= weekAgo
          : dataRange === "month"
          ? date >= monthAgo
          : dataRange === "year"
          ? date >= yearAgo
          : true;
      })
      .map((activity) => {
        const date = new Date(activity.date);
        activity.date = date.toISOString().split("T")[0];
        return activity;
      });

    filteredData.activities.forEach((activity) => {
      if (filteredData.groupedData.length === 0) {
        filteredData.groupedData.push({
          date: activity.date,
          points: activity.activity_points,
          duration: activity.duration,
          count: 1,
        });
      } else {
        const lastDate =
          filteredData.groupedData[filteredData.groupedData.length - 1].date;
        const thisDate = activity.date;

        // Merge the activities of the same day if showing week or month data, otherwise merge the activities of the same month
        if (
          dataRange === "week" || dataRange === "month"
            ? lastDate === thisDate
            : lastDate.slice(0, 7) === thisDate.slice(0, 7)
        ) {
          filteredData.groupedData[
            filteredData.groupedData.length - 1
          ].points += activity.activity_points;
          filteredData.groupedData[
            filteredData.groupedData.length - 1
          ].duration += activity.duration;
          filteredData.groupedData[
            filteredData.groupedData.length - 1
          ].count += 1;
        } else {
          filteredData.groupedData.push({
            date: activity.date,
            points: activity.activity_points,
            duration: activity.duration,
            count: 1,
          });
        }
      }
    });

    filteredData.groupedData = filteredData.groupedData.map((group) => {
      if (dataRange === "week") {
        group.date = new Date(group.date).toLocaleDateString("es-ES", {
          weekday: "long",
        });
      } else if (dataRange === "month") {
        group.date = new Date(group.date).toLocaleDateString("es-ES", {
          day: "numeric",
        });
      } else if (dataRange === "year" || dataRange === "all") {
        group.date = new Date(group.date).toLocaleDateString("es-ES", {
          month: "long",
        });
      } else {
        group.date = group.date.slice(0, 7);
      }
      return group;
    });

    const countDistributionData: { [key: string]: DonutChartCell } = {};
    const timeDistributionData: { [key: string]: DonutChartCell } = {};

    filteredData.activities.forEach((activity) => {
      if (activity.minigame in countDistributionData) {
        countDistributionData[activity.minigame].value += 1;
        timeDistributionData[activity.minigame].value += activity.duration;
      } else {
        const distributionDataKeys = Object.keys(countDistributionData).length;
        countDistributionData[activity.minigame] = {
          name: activity.minigame,
          value: 1,
          color: Colors[distributionDataKeys % Colors.length],
        };
        timeDistributionData[activity.minigame] = {
          name: activity.minigame,
          value: activity.duration,
          color: Colors[distributionDataKeys % Colors.length],
        };
      }
    });

    filteredData.playCount = Object.values(countDistributionData);
    filteredData.playTime = Object.values(timeDistributionData);

    setChartData(filteredData);
  }, [dataRange, status, data]);

  // Loading and error
  if (status === "pending") {
    return <Loader type="dots" />;
  } else if (status === "error") {
    refetch();
    return <div>Error</div>;
  }

  const minWeight = Math.min(...chartData.shapes.map((shape) => shape.weight));
  const maxWeight = Math.max(...chartData.shapes.map((shape) => shape.weight));

  // Parts of the page
  const activities = (
    <>
      <Grid.Col span={12}>
        <Title order={2}>Actividades</Title>
      </Grid.Col>
      <Card title="Puntos de actividad">
        <BarChart
          {...defaultBarChartConfig}
          data={chartData.groupedData}
          series={[{ name: "points", label: "Puntos de actividad" }]}
        />
        {chartData.activities.length === 0 && (
          <Center
            style={{
              position: "absolute",
              top: 0,
              right: 0,
              width: "100%",
              height: "100%",
              zIndex: 1,
              backgroundColor: "rgba(255, 255, 255, 0.5)",
            }}
          >
            <Text size="2.5em" c="dimmed">
              Aún no has jugado a nada
            </Text>
          </Center>
        )}
      </Card>
      <Card title="Tiempo jugado">
        <BarChart
          {...defaultBarChartConfig}
          data={chartData.groupedData}
          series={[{ name: "duration", label: "Tiempo jugado" }]}
        />
        {chartData.activities.length === 0 && (
          <Center
            style={{
              position: "absolute",
              top: 0,
              right: 0,
              width: "100%",
              height: "100%",
              zIndex: 1,
              backgroundColor: "rgba(255, 255, 255, 0.5)",
            }}
          >
            <Text size="2.5em" c="dimmed">
              Aún no has jugado a nada
            </Text>
          </Center>
        )}
      </Card>
      <Card title="Distribución de minijuegos">
        <Group justify="space-between" grow>
          <Stack align="center">
            <Title order={4}>Veces jugado</Title>
            <DonutChart
              startAngle={180}
              withLabels
              labelsType="value"
              endAngle={0}
              data={chartData.playCount}
            />
          </Stack>
          <Stack align="center">
            <Title order={4}>Tiempo jugado</Title>
            <DonutChart
              startAngle={180}
              withLabels
              labelsType="value"
              endAngle={0}
              data={chartData.playTime}
            />
          </Stack>
        </Group>
        {chartData.activities.length === 0 && (
          <Center
            style={{
              position: "absolute",
              top: 0,
              right: 0,
              width: "100%",
              height: "100%",
              zIndex: 1,
              backgroundColor: "rgba(255, 255, 255, 0.5)",
            }}
          >
            <Text size="2.5em" c="dimmed">
              Aún no has jugado a nada
            </Text>
          </Center>
        )}
      </Card>
    </>
  );

  const shapes = (
    <>
      <Grid.Col span={12}>
        <Title order={2}>Forma física</Title>
      </Grid.Col>
      <Card title="Peso">
        <LineChart
          {...defaultLineChartConfig}
          data={chartData.shapes}
          yAxisProps={{ domain: [minWeight - 5, maxWeight + 5] }}
          series={[{ name: "weight", label: "Peso" }]}
        />
      </Card>
      <Card title="IMC">
        <LineChart
          {...defaultLineChartConfig}
          data={chartData.shapes.map((shape) => {
            return {
              date: shape.date,
              bmi: (shape.weight / (shape.height / 100) ** 2).toFixed(2),
            };
          })}
          yAxisProps={{ domain: [0, 40] }}
          series={[{ name: "bmi", label: "BMI" }]}
        />
      </Card>
    </>
  );

  const filterBar = (
    <SegmentedControl
      value={dataRange}
      onChange={setDataRange}
      fullWidth
      my="sm"
      disabled={!has_data}
      data={[
        { value: "week", label: "Semana" },
        { value: "month", label: "Mes" },
        { value: "year", label: "Año" },
        { value: "all", label: "Todo" },
      ]}
    />
  );

  let content: JSX.Element;
  if (!has_data) {
    content = (
      <Card title="Aquí estarían tus estadísticas...">
        <Text>Si hubieses jugado a algo.</Text>
      </Card>
    );
  } else {
    content = (
      <>
        {activities}
        {shapes}
      </>
    );
  }

  // Render
  return (
    <>
      <canvas
        style={{
          position: "absolute",
          top: 0,
          left: 0,
          width: "100%",
          height: "100%",
          zIndex: -1,
        }}
        ref={(r) => {
          canvasRef.current = r;
        }}
      />
      <Grid columns={12} gutter="md" grow>
        <Grid.Col span={12}>
          <Title order={1}>Estadísticas</Title>
          {has_data && filterBar}
        </Grid.Col>

        {content}
      </Grid>
    </>
  );
}
