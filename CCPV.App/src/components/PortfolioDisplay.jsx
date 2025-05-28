import React, { useEffect, useState, useCallback, forwardRef, useImperativeHandle } from "react";
import { PORTFOLIO_API, BASE_URL } from "../api/config";

const COIN_PRICES_BY_SYMBOLS_API = `${BASE_URL}/api/coin/by-symbols`;

const PortfolioDisplay = forwardRef(({ userName }, ref) => {
  const [portfolios, setPortfolios] = useState([]);
  const [selectedPortfolio, setSelectedPortfolio] = useState(null);
  const [coinPrices, setCoinPrices] = useState({});
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);
  const [refreshInterval, setRefreshInterval] = useState(5); // minutes
  const [showConfig, setShowConfig] = useState(false);
  const [pendingInterval, setPendingInterval] = useState(refreshInterval);

  const fetchPortfolios = useCallback(async () => {
    try {
      setError(null);
      setLoading(true);
      const response = await fetch(PORTFOLIO_API, {
        headers: {
          'UserName': userName || 'User'
        }
      });
      if (!response.ok) throw new Error("Failed to fetch portfolio data!");
      const data = await response.json();
      // If API returns a single portfolio, wrap in array
      const arr = Array.isArray(data) ? data : (data ? [data] : []);
      setPortfolios(arr);
      setLoading(false);
      return arr;
    } catch {
      setPortfolios([]);
      setError("Unable to fetch portfolio data. Please try again later.");
      setLoading(false);
      return [];
    }
  }, [userName]);

  const fetchCoinPrices = useCallback(async (portfolioCoins) => {
    if (!portfolioCoins || portfolioCoins.length === 0) {
      setCoinPrices({});
      return {};
    }
    const symbols = Array.from(new Set(portfolioCoins.map(entry => entry.CoinSymbol || entry.coinSymbol))).join(",");
    try {
      const response = await fetch(`${COIN_PRICES_BY_SYMBOLS_API}?symbols=${symbols}`, {
        headers: {
          'UserName': userName || 'User'
        }
      });
      if (!response.ok) throw new Error("Failed to fetch coin prices!");
      const data = await response.json();
      const priceMap = {};
      data.forEach((coin) => {
        priceMap[coin.symbol] = coin.priceUsd;
      });
      setCoinPrices(priceMap);
      return priceMap;
    } catch {
      setCoinPrices({});
      setError("Unable to fetch coin prices. Please try again later.");
      return {};
    }
  }, [userName]);

  // Calculate stats for a portfolio
  const getPortfolioStats = (portfolio) => {
    if (!portfolio || !portfolio.Coins || Object.keys(coinPrices).length === 0) return null;
    let initialValue = 0;
    let currentValue = 0;
    const coinsWithChange = (portfolio.Coins || portfolio.coins).map((entry) => {
      const buyTotal = (entry.BuyPrice || entry.buyPrice) * (entry.Amount || entry.amount);
      const currentPrice = coinPrices[entry.CoinSymbol || entry.coinSymbol];
      const currentTotal = currentPrice ? currentPrice * (entry.Amount || entry.amount) : 0;
      const percentChange = (currentPrice !== undefined && currentPrice !== null && (entry.BuyPrice || entry.buyPrice) > 0)
        ? (((currentPrice - (entry.BuyPrice || entry.buyPrice)) / (entry.BuyPrice || entry.buyPrice)) * 100)
        : null;
      initialValue += buyTotal;
      currentValue += currentTotal;
      return {
        ...entry,
        currentPrice,
        currentTotal,
        percentChange,
      };
    });
    const overallChange = initialValue > 0 ? (((currentValue - initialValue) / initialValue) * 100).toFixed(2) : null;
    return { initialValue, currentValue, overallChange, coinsWithChange };
  };

  // Calculate total initial value for all portfolios
  const getTotalInitialValue = () => {
    return portfolios.reduce((sum, p) => {
      const coins = p.Coins || p.coins || [];
      return sum + coins.reduce((acc, entry) => acc + ((entry.BuyPrice || entry.buyPrice) * (entry.Amount || entry.amount)), 0);
    }, 0);
  };

  const fetchAll = useCallback(async () => {
    const arr = await fetchPortfolios();
    // Gather all unique coin symbols from all portfolios
    const allCoins = arr.flatMap(p => (p.Coins || p.coins || []));
    if (allCoins.length > 0) {
      await fetchCoinPrices(allCoins);
    }
  }, [fetchPortfolios, fetchCoinPrices]);

  // Expose a refresh method to parent via ref
  useImperativeHandle(ref, () => ({
    refresh: fetchAll
  }));

  useEffect(() => {
    fetchAll();
    const interval = setInterval(fetchAll, Math.max(refreshInterval, 2) * 60 * 1000);
    return () => clearInterval(interval);
  }, [fetchAll, refreshInterval]);

  const ConfigSlide = () => (
    <div style={{
      position: 'fixed', top: 0, right: showConfig ? 0 : '-300px', width: 300, height: '100%', background: '#fff', boxShadow: '-2px 0 8px #ccc', transition: 'right 0.3s', zIndex: 1000, padding: 24
    }}>
      <h3>Config</h3>
      <label>Refresh Interval (min, min 2): </label>
      <input type="number" min={2} value={pendingInterval} onChange={e => setPendingInterval(Number(e.target.value))} style={{ width: 60 }} />
      <div style={{ marginTop: 16 }}>
        <button onClick={() => { setRefreshInterval(Math.max(2, pendingInterval)); setShowConfig(false); }}>Save</button>
        <button style={{ marginLeft: 8 }} onClick={() => setShowConfig(false)}>Cancel</button>
      </div>
    </div>
  );

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;
  if (!portfolios || !Array.isArray(portfolios) || portfolios.length === 0) {
    return <div>The current User does not have any portfolios.</div>;
  }

  return (
    <div>
      <button style={{ float: 'right', margin: 8 }} onClick={() => setShowConfig(true)}>Config</button>
      {showConfig && <ConfigSlide />}
      <h2>Portfolios</h2>
      <div style={{marginBottom: '1em'}}>
        <strong>Initial Portfolio Value (All):</strong> ${getTotalInitialValue().toLocaleString(undefined, { maximumFractionDigits: 2 })}
      </div>
      <table style={{ margin: '1em auto', minWidth: 300 }}>
        <thead>
          <tr>
            <th>Name</th>
            <th>Coins</th>
          </tr>
        </thead>
        <tbody>
          {portfolios.map((p) => {
            const isSelected = selectedPortfolio && (selectedPortfolio.Name === (p.Name || p.name) || selectedPortfolio.name === (p.Name || p.name));
            return (
              <tr
                key={p.Name || p.name}
                style={{
                  cursor: 'pointer',
                  background: isSelected ? '#eef' : undefined,
                  color: isSelected ? '#111' : '#fff',
                }}
                onClick={() => setSelectedPortfolio(p)}
              >
                <td>{p.Name || p.name}</td>
                <td>{(p.Coins || p.coins)?.length ?? 0}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
      {selectedPortfolio && (() => {
        // Support both Coins and coins casing
        const coins = selectedPortfolio.Coins || selectedPortfolio.coins || [];
        const stats = getPortfolioStats({ ...selectedPortfolio, Coins: coins });
        if (!stats) return <div>Loading portfolio stats...</div>;
        return (
          <div style={{ marginTop: 24 }}>
            <h3>Portfolio: {selectedPortfolio.Name || selectedPortfolio.name}</h3>
            <div><strong>Initial Portfolio Value:</strong> ${stats.initialValue.toLocaleString(undefined, { maximumFractionDigits: 2 })}</div>
            <div><strong>Current Portfolio Value:</strong> ${stats.currentValue.toLocaleString(undefined, { maximumFractionDigits: 2 })}</div>
            <div><strong>Overall Change:</strong> {stats.overallChange}%</div>
            <table style={{ margin: '1em auto' }}>
              <thead>
                <tr>
                  <th>Coin</th>
                  <th>Buy Price</th>
                  <th>Amount</th>
                  <th>Current Price</th>
                  <th>Current Value</th>
                  <th>Change (%)</th>
                </tr>
              </thead>
              <tbody>
                {stats.coinsWithChange
                  .slice() // copy array
                  .sort((a, b) => (b.currentTotal || 0) - (a.currentTotal || 0))
                  .map((coin) => (
                    <tr key={coin.CoinSymbol || coin.coinSymbol}>
                      <td>{coin.CoinSymbol || coin.coinSymbol}</td>
                      <td>{(coin.BuyPrice || coin.buyPrice) !== undefined && (coin.BuyPrice || coin.buyPrice) !== null ? Number(coin.BuyPrice || coin.buyPrice).toLocaleString(undefined, { maximumFractionDigits: 10, minimumFractionDigits: 4 }) : '0'}</td>
                      <td>{coin.Amount || coin.amount}</td>
                      <td>{coin.currentPrice !== undefined ? coin.currentPrice : 'N/A'}</td>
                      <td>{coin.currentTotal !== undefined ? coin.currentTotal.toLocaleString(undefined, { maximumFractionDigits: 2 }) : 'N/A'}</td>
                      <td>{coin.percentChange !== null ? (
                        <span style={{ color: coin.percentChange > 0 ? 'limegreen' : coin.percentChange < 0 ? 'red' : 'inherit', fontWeight: 600, display: 'flex', alignItems: 'center', gap: 4 }}>
                          {coin.percentChange > 0 && <span style={{fontSize: '1.1em'}}>▲</span>}
                          {coin.percentChange < 0 && <span style={{fontSize: '1.1em'}}>▼</span>}
                          {Number(coin.percentChange).toLocaleString(undefined, { maximumFractionDigits: 8, minimumFractionDigits: 2 })}%
                        </span>
                      ) : 'N/A'}</td>
                    </tr>
                  ))}
              </tbody>
            </table>
            <div style={{ marginTop: '1em' }}>
              <small>Auto-refreshes every {Math.max(refreshInterval, 2)} minutes.</small>
            </div>
          </div>
        );
      })()}
    </div>
  );
});

export default PortfolioDisplay;