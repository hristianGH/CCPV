import React, { useEffect, useState } from "react";
import { PORTFOLIO_API } from "../api/config";

const PortfolioDisplay = () => {
  const [portfolio, setPortfolio] = useState(null);

  const fetchPortfolio = async () => {
    try {
      const response = await fetch(PORTFOLIO_API);
      if (!response.ok) throw new Error("Failed to fetch portfolio data!");
      const data = await response.json();
      setPortfolio(data);
    } catch (error) {
      console.error(error);
      alert("Error fetching portfolio data.");
    }
  };

  useEffect(() => {
    fetchPortfolio();
    const interval = setInterval(fetchPortfolio, 5 * 60 * 1000); // Refresh every 5 minutes
    return () => clearInterval(interval);
  }, []);

  if (!portfolio) return <div>Loading...</div>;

  return (
    <div>
      <h2>Portfolio</h2>
      <table>
        <thead>
          <tr>
            <th>Coin</th>
            <th>Initial Value</th>
            <th>Current Value</th>
            <th>Change (%)</th>
          </tr>
        </thead>
        <tbody>
          {portfolio.map((coin) => (
            <tr key={coin.name}>
              <td>{coin.name}</td>
              <td>{coin.initialValue}</td>
              <td>{coin.currentValue}</td>
              <td>{coin.changePercentage}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default PortfolioDisplay;