# 🛍️ eShop Monolith

Repositório demonstrativo da evolução arquitetural de uma aplicação e-commerce, migrando de uma estrutura legada para um monólito modular moderno.

---

## 🔄 Evolução: Add Cache (Redis & Output Cache)

Para otimizar a performance e consistência da nossa arquitetura modular, implementamos uma estratégia de cache centralizada na **API de Catálogo** utilizando **Redis**.

### 🧩 Conceitos Chave Implementados:

- **TTL (Time to Live):** Definimos o tempo de vida dos objetos no cache. Com a implementação de invalidação ativa, podemos trabalhar com TTLs mais longos, garantindo alta disponibilidade sem sacrificar a atomicidade dos dados.
- **Cache Invalidation:** Utilizamos a técnica de **Tag-based Eviction** (via tag `"products"`). Isso permite que, ao realizar qualquer operação de escrita (POST, PUT, DELETE), o cache de listagem seja invalidado de forma cirúrgica e imediata.
- **Cache Stampede (Dog-piling):** Mitigado através do mecanismo intrínseco de *locking* do Output Cache do ASP.NET Core, que garante que apenas a primeira requisição "carregue" o cache, evitando sobrecarga no banco de dados.
- **Cache Key Design:** Chaves geradas automaticamente baseadas em rotas e parâmetros, garantindo que diferentes visões do catálogo (filtros, paginação) sejam cacheadas corretamente e de forma isolada.

### 🎯 Problemas que o cache resolve nesta arquitetura:
- **Latência:** Respostas em milissegundos ao evitar consultas repetitivas ao PostgreSQL.
- **Concorrência:** Proteção do banco de dados durante picos de tráfego, movendo a carga de leitura para o Redis.
- **Custo:** Redução de I/O e processamento no banco de dados, permitindo escalabilidade vertical e horizontal mais eficiente.
- **Disponibilidade:** Camada de resiliência que permite a leitura de dados mesmo sob alta pressão nos serviços de persistência.

---

## 📈 O Desafio da Escalabilidade em Sistemas Modernos

À medida que uma aplicação ganha tração, o crescimento do volume de usuários simultâneos deixa de ser um indicador de sucesso para se tornar um desafio de engenharia. Operações de leitura intensiva, como a navegação em catálogos de produtos, tornam-se gargalos críticos que podem comprometer a experiência do usuário e a viabilidade do negócio.

Nesse contexto, a **escalabilidade** transcende a classificação de um simples requisito não funcional; ela se torna um pilar vital para a sobrevivência e sustentabilidade do projeto.

### 1. Definição
**Escalabilidade** é a propriedade de um sistema, rede ou processo que indica sua habilidade de reagir e se adaptar a um aumento crescente na carga de trabalho (demanda) sem perda de desempenho ou disponibilidade.

### 2. Dimensões da Escalabilidade
Para endereçar esse crescimento, a arquitetura pode seguir dois caminhos principais:

#### I. Escalabilidade Vertical (Scaling Up)
Consiste em aumentar o poder computacional de um único nó do sistema. Isso significa adicionar mais CPU, memória (RAM) ou armazenamento a um servidor já existente.
- **Vantagem:** Baixa complexidade de software; não exige mudanças na lógica de distribuição.
- **Limitação:** Existe um "teto" físico (limite de hardware) e o custo cresce exponencialmente à medida que os componentes se tornam mais potentes.

#### II. Escalabilidade Horizontal (Scaling Out)
Refere-se à expansão da infraestrutura através da adição de múltiplas instâncias ou servidores trabalhando em paralelo.
- **Vantagem:** Praticamente ilimitada e resiliente (se um nó falha, outros assumem). É a base para arquiteturas de microsserviços e nuvem.
- **Desafio:** Introduz complexidade na coordenação, exigindo o uso de Load Balancers (balanceadores de carga) e consistência de dados distribuída.

---

## 🛠️ Metrificando seu sistema

Para que a escalabilidade seja aplicada com precisão, é impossível depender de suposições; é necessário **metrificar**. Metrificar um sistema consiste em implementar uma camada de **observabilidade** que transforme o comportamento da aplicação em dados quantificáveis, permitindo identificar gargalos antes que eles causem indisponibilidade.

Aqui estão os pilares fundamentais para uma metrificação eficaz:

### 1. Indicadores de Performance (KPIs Técnicos)
Para entender a saúde do seu sistema, você deve monitorar métricas de infraestrutura e de aplicação:
- **Throughput (Vazão):** Quantas requisições seu sistema processa por segundo.
- **Latência:** O tempo de resposta para o usuário final (medido geralmente em percentis como p95 ou p99 para capturar anomalias).
- **Taxa de Erros:** A porcentagem de requisições que resultam em falhas (HTTP 5xx, timeouts).
- **Saturação de Recursos:** O consumo percentual de CPU, memória e largura de banda de rede.

### 2. O Papel da Metrificação na Tomada de Decisão
Sem métricas, o processo de escalonamento torna-se caro e ineficiente. Dados precisos permitem configurar o **Auto-scaling** (ajuste automático de instâncias) com base em gatilhos reais, como *"se a CPU exceder 70% por 2 minutos, adicione uma nova instância"*. Além disso, a metrificação é a base para o planejamento de capacidade (**Capacity Planning**), permitindo prever quanto a infraestrutura precisará crescer para suportar eventos de alta demanda, como uma Black Friday.

### 3. As Três Dimensões da Observabilidade
Um sistema bem metrificado utiliza o "tripé" da observabilidade:
- **Métricas:** Dados agregados e temporais (ex: uso de RAM).
- **Logs:** Registros detalhados de eventos específicos para depuração.
- **Tracing:** O rastreamento de uma requisição enquanto ela viaja entre diferentes módulos ou microsserviços.

---

## 🔭 Ferramentas e Implementação

### 1. Coleta e Padronização (O "Cérebro")
- **OpenTelemetry (.NET SDK):** É o padrão atual. Em vez de se prender a uma ferramenta específica, você usa as bibliotecas do OpenTelemetry para gerar métricas, logs e rastreios (traces). Ele "traduz" os dados da sua aplicação para qualquer ferramenta de visualização.
- **System.Diagnostics.Metrics:** Bibliotecas nativas do .NET (desde o .NET 6) que permitem criar contadores e medidores de performance sem depender de pacotes externos.

### 2. Monitoramento e Visualização (Os "Dashboards")
- **Grafana:** A ferramenta de visualização soberana. Você a utiliza para criar dashboards em tempo real que mostram a saúde do seu sistema modular.
- **Prometheus:** Um banco de dados de séries temporais que "puxa" as métricas da sua aplicação .NET e as armazena para que o Grafana possa exibi-las.
- **Azure Monitor / Application Insights:** Se você estiver no ecossistema Cloud da Microsoft, esta é a escolha "out-of-the-box". Ele oferece o Application Map, que visualiza automaticamente como seus módulos se comunicam e onde estão os gargalos.

### 3. Diagnóstico em Tempo Real (A "Linha de Comando")
Se você precisar investigar um problema de escalabilidade agora mesmo em um servidor, a Microsoft oferece ferramentas CLI (Command Line Interface) poderosas:
- **dotnet-counters:** Exibe contadores de performance (como uso de CPU, taxa de requisições HTTP e GC Heap size) em tempo real no terminal.
- **dotnet-trace:** Coleta rastreios de performance de um processo em execução para analisar o que está consumindo tempo de CPU.
- **dotnet-monitor:** Facilita a coleta de arquivos de diagnóstico (como dumps de memória) em ambientes de produção ou containers (Docker/Kubernetes).

---

## 🚀 Exemplo Prático de Fluxo

1. Sua aplicação .NET gera dados via **OpenTelemetry**.
2. O **Prometheus** coleta esses dados a cada 15 segundos.
3. O **Grafana** exibe um gráfico de *"Tempo de Resposta por Módulo"*.
4. Se a latência subir, você usa o **dotnet-counters** para ver se a memória RAM está saturada.

> **Nota:** Como estamos usando o **Aspire**, essa observabilidade já está configurada por padrão!
