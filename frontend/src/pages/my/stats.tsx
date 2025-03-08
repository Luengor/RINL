import { Grid, Loader, SegmentedControl, Title } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { getActivitiesActivityGet, getShapesShapeGet } from "../../client";
import { ActivityUuid, ShapeUuid } from "../../client";
import { LineChart, LineChartProps } from "@mantine/charts";
import { useEffect, useState } from "react";

interface Data {
  activity: ActivityUuid[];
  shape: ShapeUuid[];
}

export function Col({ children }: { children: React.ReactNode }) {
  return (
    <Grid.Col span={{sm: 12, lg: 6}} px={{sm: 'sm', lg: 'md'}}>
      {children}
    </Grid.Col>
  )
}

const defaultChartConfig: LineChartProps = {
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
      } as Data;
    },
    staleTime: 1000 * 60 * 10,
  })

  const [dataRange, setDataRange] = useState("week");
  const [chartData, setChartData] = useState(data);

  useEffect(() => {
    if (status !== 'success') return;

    const now = new Date();
    const weekAgo = new Date(now.getTime() - 1000 * 60 * 60 * 24 * 7);
    const monthAgo = new Date(now.getTime() - 1000 * 60 * 60 * 24 * 30);
    const yearAgo = new Date(now.getTime() - 1000 * 60 * 60 * 24 * 365);

    const filteredData: Data = {
      activity: data.activity.filter((activity) => {
        const date = new Date(activity.date);
        return dataRange === "week" ? date >= weekAgo :
               dataRange === "month" ? date >= monthAgo :
               dataRange === "year" ? date >= yearAgo :
               true;
      }).map((activity) => {
        const date = new Date(activity.date);
        activity.date = date.toISOString().split('T')[0];
        return activity;
      }),
      shape: data.shape.filter((shape) => {
        const date = new Date(shape.date);
        return dataRange === "week" ? date >= weekAgo :
               dataRange === "month" ? date >= monthAgo :
               dataRange === "year" ? date >= yearAgo :
               true;
      }).map((shape) => {
        const date = new Date(shape.date);
        shape.date = date.toISOString().split('T')[0];
        return shape;
      })
    };
    setChartData(filteredData);
  }, [dataRange, status]);

  // Loading and error
  if (status === 'pending') {
    return <Loader type="dots"/>
  } else if (status === 'error') {
    refetch();
    return <div>Error</div>
  }

  // Data
  const minWeight = Math.min(...data.shape.map((shape) => shape.weight));
  const maxWeight = Math.max(...data.shape.map((shape) => shape.weight));

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
      
      <Grid.Col span={12}>
        <Title order={2}>Forma física</Title>
      </Grid.Col>
      <Col>
        <Title order={3} mb="xs">Peso</Title>
        <LineChart
          {...defaultChartConfig}

          data={chartData.shape}
          yAxisProps={{ 
            domain: [minWeight - 5, maxWeight + 5],
          }}
          series={[
            { name: "weight", label: "Peso" },
          ]}
        />
      </Col>
      <Col>
        <Title order={3} mb="xs">IMC</Title>
        <LineChart
          {...defaultChartConfig}

          data={chartData.shape.map((shape) => {
            return {
              date: shape.date,
              bmi: (shape.weight / (shape.height / 100) ** 2).toFixed(2),
            }
          })}
          yAxisProps={{
            domain: [0, 40],
          }}
          series={[
            { name: "bmi", label: "BMI" },
          ]}
        />
      </Col>
    </Grid>
  )
}