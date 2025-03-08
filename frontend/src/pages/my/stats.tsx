import { ActionIcon, Center, Grid, Group, Loader, Paper, SegmentedControl, Stack, Title } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { getActivitiesActivityGet, getShapesShapeGet } from "../../client";
import { ActivityUuid, ShapeUuid } from "../../client";
import { BarChart, BarChartProps, DonutChart, DonutChartCell, LineChart, LineChartProps } from "@mantine/charts";
import { useEffect, useState } from "react";

interface ApiData {
  activity: ActivityUuid[];
  shape: ShapeUuid[];
}

interface PreprocessedData {
  activities: ActivityUuid[]; 
  shapes: ShapeUuid[];
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
    <Grid.Col span={{sm: 12, lg: 6}} px={{sm: 'sm', lg: 'md'}}>
      {children}
    </Grid.Col>
  )
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
    }
  }
};

const Colors = [
  "blue",
  "orange",
  "purple",
  "cyan",
  "pink",
  "gray"
]

const defaultBarChartConfig: BarChartProps = {
  data: [],
  series: [],

  h: "25vh",
  dataKey: "date",
  xAxisProps: {
    padding: {
      left: 10,
      right: 20,
    }
  }
};

function Card({ title, children }: { title: string, children: React.ReactNode }) {
  return (
    <Col>
      <Paper shadow="md" p="md">
        <Title order={3} mb="xl">{title}</Title>
        {children}
      </Paper>
    </Col>
  )
}

export default function Stats() {
  // Get activity and shape data
  const { data, status, refetch } = useQuery({
    queryKey: ['activity-shape-data'],
    queryFn: async () => {
      const activity_data = getActivitiesActivityGet(); 
      const shape_data = getShapesShapeGet();
      const data = await Promise.all([activity_data, shape_data]);
      return {
        activity: data[0].data,
        shape: data[1].data,
      } as ApiData;
    },
    placeholderData: { activity: [], shape: [] },
    staleTime: 1000 * 60 * 10,
  })

  const [dataRange, setDataRange] = useState("week");
  const [chartData, setChartData] = useState<PreprocessedData>({
    activities: [],
    shapes: [],
    groupedData: [],
    playCount: [],
    playTime: [],
  });

  useEffect(() => {
    if (status !== 'success') return;

    const now = new Date();
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

    filteredData.shapes = data.shape.filter((shape) => {
      const date = new Date(shape.date);
      return dataRange === "week" ? date >= weekAgo :
             dataRange === "month" ? date >= monthAgo :
             dataRange === "year" ? date >= yearAgo :
             true;
    }).sort((a, b) => {
      return new Date(a.date).getTime() - new Date(b.date).getTime();
    }).map((shape) => {
      const date = new Date(shape.date);
      shape.date = date.toISOString().split('T')[0];
      return shape;
    });

    filteredData.activities = data.activity.filter((activity) => {
      const date = new Date(activity.date);
      return dataRange === "week" ? date >= weekAgo :
             dataRange === "month" ? date >= monthAgo :
             dataRange === "year" ? date >= yearAgo :
             true;
    }).sort((a, b) => {
      return new Date(a.date).getTime() - new Date(b.date).getTime();
    }).map((activity) => {
      const date = new Date(activity.date);
      activity.date = date.toISOString().split('T')[0];
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
        const lastDate = filteredData.groupedData[filteredData.groupedData.length - 1].date;
        const thisDate = activity.date;

        // Merge the activities of the same day if showing week or month data, otherwise merge the activities of the same month 
        if (dataRange === "week" || dataRange === "month" ? lastDate === thisDate : lastDate.slice(0, 7) === thisDate.slice(0, 7)) {
          filteredData.groupedData[filteredData.groupedData.length - 1].points += activity.activity_points;
          filteredData.groupedData[filteredData.groupedData.length - 1].duration += activity.duration;
          filteredData.groupedData[filteredData.groupedData.length - 1].count += 1;
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
      group.date = dataRange === "week" || dataRange === "month" ? group.date.slice(8) : group.date.slice(0, 7);
      return group;
    });

    const countDistributionData: { [key: string]: DonutChartCell} = {};
    const timeDistributionData: { [key: string]: DonutChartCell} = {};

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
        }
        timeDistributionData[activity.minigame] = {
          name: activity.minigame,
          value: activity.duration,
          color: Colors[distributionDataKeys % Colors.length]
        }
      }
    });

    filteredData.playCount = Object.values(countDistributionData);
    filteredData.playTime = Object.values(timeDistributionData);

    setChartData(filteredData);
    console.log(filteredData);
  }, [dataRange, status, data]);

  // Loading and error
  if (status === 'pending') {
    return <Loader type="dots"/>
  } else if (status === 'error') {
    refetch();
    return <div>Error</div>
  }

  // Data
  const minWeight = Math.min(...chartData.shapes.map((shape) => shape.weight));
  const maxWeight = Math.max(...chartData.shapes.map((shape) => shape.weight));

  // Render
  return (
    <Grid columns={12} gutter="md" grow>
      <Grid.Col span={12}>
        <Title order={1}>Estadísticas</Title>
        <SegmentedControl
          value={dataRange}
          onChange={setDataRange}
          fullWidth
          my="sm"
          data={[
            { value: "week", label: "Semana" },
            { value: "month", label: "Mes" },
            { value: "year", label: "Año" },
            { value: "all", label: "Todo" }]
        }/>
      </Grid.Col>

      <Grid.Col span={12}>
        <Title order={2}>Actividades</Title>
      </Grid.Col>
      <Card title="Puntos de actividad">
        <BarChart
          {...defaultBarChartConfig}
          data={chartData.groupedData}
          series={[{ name: "points", label: "Puntos de actividad" }]}
        />
      </Card>
      <Card title="Tiempo jugado">
        <BarChart
          {...defaultBarChartConfig}
          data={chartData.groupedData}
          series={[{ name: "duration", label: "Tiempo jugado" }]}
        />
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
      </Card>
      
      <Grid.Col span={12}>
        <Title order={2}>Forma física</Title>
      </Grid.Col>
      <Card title="Peso">
        <LineChart
          {...defaultLineChartConfig}

          data={chartData.shapes}
          yAxisProps={{domain: [minWeight - 5, maxWeight + 5]}}
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
            }
          })}
          yAxisProps={{ domain: [0, 40] }}
          series={[{ name: "bmi", label: "BMI" }]}
        />
      </Card>
    </Grid>
  )
}