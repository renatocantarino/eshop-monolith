# 🛍️ eShop Monolith


## 🔄 Evolução: N-Tier ➔ Modular Monolith

Realizamos a migração de uma arquitetura em camadas técnicas (N-Tier) para um **Modular Monolith**, organizando o código por **contextos de negócio** (Features/Modules) em vez de camadas horizontais (UI, BLL, DAL).

### Prós e Contras da Abordagem

| | Modular Monolith |
|---|---|
| **✅ Prós** | **Alta Coesão**: Tudo relacionado a uma funcionalidade (ex: Pedidos) reside no mesmo módulo.<br>**Limites Claros**: Impede acoplamento acidental entre domínios distintos.<br>**Manutenibilidade**: Alterações em um módulo têm impacto isolado.<br>**Transição para Microserviços**: Facilita a extração futura de módulos independentes. |
| **❌ Contras** | **Complexidade Inicial**: Exige maior disciplina na definição de fronteiras e contratos.<br>**Curva de Aprendizado**: Requer mudança de mentalidade de organização técnica para funcional.<br>**Duplicação Consciente**: Pode haver repetição de código para evitar acoplamento entre módulos. |




