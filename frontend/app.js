let API = localStorage.getItem('api_locadora') || 'http://localhost:5084';

let listaFabricantes = [];
let listaVeiculos = [];
let listaClientes = [];
let listaAlugueis = [];

let mFab, mVei, mCli, mAlu, mDev;

window.onload = function () {
  document.getElementById('urlApi').value = API;
  mFab = new bootstrap.Modal(document.getElementById('modalFab'));
  mVei = new bootstrap.Modal(document.getElementById('modalVei'));
  mCli = new bootstrap.Modal(document.getElementById('modalCli'));
  mAlu = new bootstrap.Modal(document.getElementById('modalAlu'));
  mDev = new bootstrap.Modal(document.getElementById('modalDev'));

  carregarFabricantes();
};

function salvarUrlApi() {
  API = document.getElementById('urlApi').value.trim().replace(/\/$/, '');
  localStorage.setItem('api_locadora', API);
  mostrarMsg('URL da API atualizada para: ' + API, 'success');
  carregarFabricantes();
}

function mostrarMsg(texto, tipo = 'success') {
  const el = document.getElementById('alerta');
  el.className = 'alert alert-' + tipo;
  el.textContent = texto;
  setTimeout(() => el.classList.add('d-none'), 4000);
}

async function req(rota, metodo = 'GET', corpo = null) {
  const opcoes = {
    method: metodo,
    headers: { 'Content-Type': 'application/json' }
  };
  if (corpo) opcoes.body = JSON.stringify(corpo);

  const resp = await fetch(API + rota, opcoes);
  if (resp.status === 204) return null;

  const texto = await resp.text();
  let dados = null;
  try { dados = texto ? JSON.parse(texto) : null; } catch { dados = texto; }

  if (!resp.ok) {
    let erro = 'Erro ' + resp.status;
    if (dados && dados.mensagem) erro = dados.mensagem;
    else if (dados && dados.errors) erro = Object.values(dados.errors).flat().join(' | ');
    throw new Error(erro);
  }
  return dados;
}

function abrirSecao(nome) {
  document.querySelectorAll('.secao').forEach(s => s.classList.remove('ativa'));
  document.querySelectorAll('.nav-link').forEach(a => a.classList.remove('active'));

  document.getElementById('sec-' + nome).classList.add('ativa');
  const link = document.querySelector(`.nav-link[data-tab="${nome}"]`);
  if (link) link.classList.add('active');

  if (nome === 'fabricantes') carregarFabricantes();
  if (nome === 'veiculos') carregarVeiculos();
  if (nome === 'clientes') carregarClientes();
  if (nome === 'alugueis') carregarAlugueis();
  if (nome === 'consultas') {
    buscarConsulta3();
    buscarConsulta4();
    buscarConsulta5();
  }
}

function fmtData(d) {
  if (!d) return '-';
  return new Date(d).toLocaleDateString('pt-BR');
}

function fmtDinheiro(v) {
  return Number(v || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function textoStatusVei(s) {
  const mapa = { 1: 'Disponível', 2: 'Alugado', 3: 'Manutenção', 4: 'Inativo', Disponivel: 'Disponível', Alugado: 'Alugado', Manutencao: 'Manutenção', Inativo: 'Inativo' };
  return mapa[s] || s;
}

// ==================== FABRICANTES ====================
async function carregarFabricantes() {
  try {
    listaFabricantes = await req('/api/fabricantes');
    renderFabricantes();
  } catch (e) {
    mostrarMsg(e.message, 'danger');
  }
}

function renderFabricantes() {
  const nome = document.getElementById('fFabNome').value.toLowerCase();
  const pais = document.getElementById('fFabPais').value.toLowerCase();
  const ativo = document.getElementById('fFabAtivo').value;

  const filtrados = listaFabricantes.filter(f =>
    (!nome || (f.nome || '').toLowerCase().includes(nome)) &&
    (!pais || (f.paisOrigem || '').toLowerCase().includes(pais)) &&
    (ativo === '' || String(f.ativo) === ativo)
  );

  const tb = document.getElementById('tbFabricantes');
  if (!filtrados.length) {
    tb.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Nenhum registro.</td></tr>';
    return;
  }

  tb.innerHTML = filtrados.map(f => `
    <tr>
      <td>${f.fabricanteId}</td>
      <td>${f.nome}</td>
      <td>${f.paisOrigem || '-'}</td>
      <td>${f.ativo ? 'Sim' : 'Não'}</td>
      <td>${f.quantidadeVeiculos ?? 0}</td>
      <td>
        <button class="btn btn-sm btn-outline-primary" onclick="editarFabricante(${f.fabricanteId})">Editar</button>
        <button class="btn btn-sm btn-outline-danger" onclick="excluirFabricante(${f.fabricanteId})">Excluir</button>
      </td>
    </tr>
  `).join('');
}

function limparFiltroFab() {
  document.getElementById('fFabNome').value = '';
  document.getElementById('fFabPais').value = '';
  document.getElementById('fFabAtivo').value = '';
  renderFabricantes();
}

function novoFabricante() {
  document.getElementById('fabId').value = '';
  document.getElementById('fabNome').value = '';
  document.getElementById('fabPais').value = '';
  document.getElementById('fabAtivo').checked = true;
  document.getElementById('tituloModalFab').textContent = 'Novo Fabricante';
  mFab.show();
}

async function editarFabricante(id) {
  try {
    const f = await req('/api/fabricantes/' + id);
    document.getElementById('fabId').value = f.fabricanteId;
    document.getElementById('fabNome').value = f.nome;
    document.getElementById('fabPais').value = f.paisOrigem || '';
    document.getElementById('fabAtivo').checked = f.ativo;
    document.getElementById('tituloModalFab').textContent = 'Editar Fabricante #' + id;
    mFab.show();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function salvarFabricante(e) {
  e.preventDefault();
  const id = document.getElementById('fabId').value;
  const body = {
    nome: document.getElementById('fabNome').value.trim(),
    paisOrigem: document.getElementById('fabPais').value.trim() || null,
    ativo: document.getElementById('fabAtivo').checked
  };
  try {
    if (id) await req('/api/fabricantes/' + id, 'PUT', body);
    else await req('/api/fabricantes', 'POST', body);
    mFab.hide();
    mostrarMsg('Fabricante salvo com sucesso!');
    carregarFabricantes();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function excluirFabricante(id) {
  if (!confirm('Excluir fabricante #' + id + '?')) return;
  try {
    await req('/api/fabricantes/' + id, 'DELETE');
    mostrarMsg('Fabricante excluído!');
    carregarFabricantes();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

// ==================== VEÍCULOS ====================
async function carregarVeiculos() {
  try {
    const fabs = await req('/api/fabricantes');
    const selFiltro = document.getElementById('fVeiFab');
    const atual = selFiltro.value;
    selFiltro.innerHTML = '<option value="">Fabricante...</option>' +
      fabs.map(f => `<option value="${f.fabricanteId}">${f.nome}</option>`).join('');
    selFiltro.value = atual;

    document.getElementById('veiFabricanteId').innerHTML =
      '<option value="">Selecione...</option>' +
      fabs.map(f => `<option value="${f.fabricanteId}">${f.nome}</option>`).join('');

    listaVeiculos = await req('/api/veiculos');
    renderVeiculos();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

function renderVeiculos() {
  const placa = document.getElementById('fVeiPlaca').value.toLowerCase();
  const modelo = document.getElementById('fVeiModelo').value.toLowerCase();
  const fab = document.getElementById('fVeiFab').value;
  const status = document.getElementById('fVeiStatus').value;

  const filtrados = listaVeiculos.filter(v =>
    (!placa || (v.placa || '').toLowerCase().includes(placa)) &&
    (!modelo || (v.modelo || '').toLowerCase().includes(modelo)) &&
    (!fab || String(v.fabricanteId) === fab) &&
    (!status || String(v.status).toLowerCase() === status.toLowerCase())
  );

  const tb = document.getElementById('tbVeiculos');
  if (!filtrados.length) {
    tb.innerHTML = '<tr><td colspan="8" class="text-center text-muted">Nenhum registro.</td></tr>';
    return;
  }

  tb.innerHTML = filtrados.map(v => `
    <tr>
      <td>${v.veiculoId}</td>
      <td><b>${v.placa}</b></td>
      <td>${v.modelo}</td>
      <td>${v.fabricanteNome || '-'}</td>
      <td>${v.anoFabricacao}</td>
      <td>${v.quilometragem}</td>
      <td>${textoStatusVei(v.status)}</td>
      <td>
        <button class="btn btn-sm btn-outline-primary" onclick="editarVeiculo(${v.veiculoId})">Editar</button>
        <button class="btn btn-sm btn-outline-danger" onclick="excluirVeiculo(${v.veiculoId})">Excluir</button>
      </td>
    </tr>
  `).join('');
}

function limparFiltroVei() {
  document.getElementById('fVeiPlaca').value = '';
  document.getElementById('fVeiModelo').value = '';
  document.getElementById('fVeiFab').value = '';
  document.getElementById('fVeiStatus').value = '';
  renderVeiculos();
}

function novoVeiculo() {
  document.getElementById('veiId').value = '';
  document.getElementById('veiPlaca').value = '';
  document.getElementById('veiModelo').value = '';
  document.getElementById('veiFabricanteId').value = '';
  document.getElementById('veiAno').value = 2024;
  document.getElementById('veiKm').value = 0;
  document.getElementById('veiCor').value = '';
  document.getElementById('veiChassi').value = '';
  document.getElementById('veiStatus').value = 1;
  document.getElementById('tituloModalVei').textContent = 'Novo Veículo';
  mVei.show();
}

async function editarVeiculo(id) {
  try {
    const v = await req('/api/veiculos/' + id);
    const mapa = { Disponivel: 1, Alugado: 2, Manutencao: 3, Inativo: 4 };
    document.getElementById('veiId').value = v.veiculoId;
    document.getElementById('veiPlaca').value = v.placa;
    document.getElementById('veiModelo').value = v.modelo;
    document.getElementById('veiFabricanteId').value = v.fabricanteId;
    document.getElementById('veiAno').value = v.anoFabricacao;
    document.getElementById('veiKm').value = v.quilometragem;
    document.getElementById('veiCor').value = v.cor || '';
    document.getElementById('veiChassi').value = v.chassi || '';
    document.getElementById('veiStatus').value = mapa[v.status] || 1;
    document.getElementById('tituloModalVei').textContent = 'Editar Veículo #' + id;
    mVei.show();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function salvarVeiculo(e) {
  e.preventDefault();
  const id = document.getElementById('veiId').value;
  const body = {
    placa: document.getElementById('veiPlaca').value.trim().toUpperCase(),
    modelo: document.getElementById('veiModelo').value.trim(),
    fabricanteId: Number(document.getElementById('veiFabricanteId').value),
    anoFabricacao: Number(document.getElementById('veiAno').value),
    quilometragem: Number(document.getElementById('veiKm').value),
    cor: document.getElementById('veiCor').value.trim() || null,
    chassi: document.getElementById('veiChassi').value.trim() || null,
    status: Number(document.getElementById('veiStatus').value)
  };
  try {
    if (id) await req('/api/veiculos/' + id, 'PUT', body);
    else await req('/api/veiculos', 'POST', body);
    mVei.hide();
    mostrarMsg('Veículo salvo com sucesso!');
    carregarVeiculos();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function excluirVeiculo(id) {
  if (!confirm('Excluir veículo #' + id + '?')) return;
  try {
    await req('/api/veiculos/' + id, 'DELETE');
    mostrarMsg('Veículo excluído!');
    carregarVeiculos();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

// ==================== CLIENTES ====================
async function carregarClientes() {
  try {
    listaClientes = await req('/api/clientes');
    renderClientes();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

function renderClientes() {
  const nome = document.getElementById('fCliNome').value.toLowerCase();
  const cpf = document.getElementById('fCliCpf').value.replace(/\D/g, '');
  const email = document.getElementById('fCliEmail').value.toLowerCase();

  const filtrados = listaClientes.filter(c =>
    (!nome || (c.nome || '').toLowerCase().includes(nome)) &&
    (!cpf || (c.cpf || '').includes(cpf)) &&
    (!email || (c.email || '').toLowerCase().includes(email))
  );

  const tb = document.getElementById('tbClientes');
  if (!filtrados.length) {
    tb.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Nenhum registro.</td></tr>';
    return;
  }

  tb.innerHTML = filtrados.map(c => `
    <tr>
      <td>${c.clienteId}</td>
      <td>${c.nome}</td>
      <td>${c.cpf}</td>
      <td>${c.email}</td>
      <td>${c.telefone || '-'}</td>
      <td>${c.numeroCnh || '-'}</td>
      <td>
        <button class="btn btn-sm btn-outline-primary" onclick="editarCliente(${c.clienteId})">Editar</button>
        <button class="btn btn-sm btn-outline-danger" onclick="excluirCliente(${c.clienteId})">Excluir</button>
      </td>
    </tr>
  `).join('');
}

function limparFiltroCli() {
  document.getElementById('fCliNome').value = '';
  document.getElementById('fCliCpf').value = '';
  document.getElementById('fCliEmail').value = '';
  renderClientes();
}

function novoCliente() {
  document.getElementById('cliId').value = '';
  document.getElementById('cliNome').value = '';
  document.getElementById('cliCpf').value = '';
  document.getElementById('cliEmail').value = '';
  document.getElementById('cliTel').value = '';
  document.getElementById('cliCnh').value = '';
  document.getElementById('cliNasc').value = '';
  document.getElementById('tituloModalCli').textContent = 'Novo Cliente';
  mCli.show();
}

async function editarCliente(id) {
  try {
    const c = await req('/api/clientes/' + id);
    document.getElementById('cliId').value = c.clienteId;
    document.getElementById('cliNome').value = c.nome;
    document.getElementById('cliCpf').value = c.cpf;
    document.getElementById('cliEmail').value = c.email;
    document.getElementById('cliTel').value = c.telefone || '';
    document.getElementById('cliCnh').value = c.numeroCnh || '';
    document.getElementById('cliNasc').value = c.dataNascimento ? c.dataNascimento.substring(0, 10) : '';
    document.getElementById('tituloModalCli').textContent = 'Editar Cliente #' + id;
    mCli.show();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function salvarCliente(e) {
  e.preventDefault();
  const id = document.getElementById('cliId').value;
  const body = {
    nome: document.getElementById('cliNome').value.trim(),
    cpf: document.getElementById('cliCpf').value.replace(/\D/g, ''),
    email: document.getElementById('cliEmail').value.trim(),
    telefone: document.getElementById('cliTel').value.trim() || null,
    numeroCnh: document.getElementById('cliCnh').value.trim() || null,
    dataNascimento: document.getElementById('cliNasc').value || null
  };
  try {
    if (id) await req('/api/clientes/' + id, 'PUT', body);
    else await req('/api/clientes', 'POST', body);
    mCli.hide();
    mostrarMsg('Cliente salvo com sucesso!');
    carregarClientes();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function excluirCliente(id) {
  if (!confirm('Excluir cliente #' + id + '?')) return;
  try {
    await req('/api/clientes/' + id, 'DELETE');
    mostrarMsg('Cliente excluído!');
    carregarClientes();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

// ==================== ALUGUÉIS ====================
async function carregarAlugueis() {
  try {
    listaAlugueis = await req('/api/alugueis');
    renderAlugueis();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

function renderAlugueis() {
  const cli = document.getElementById('fAluCli').value.toLowerCase();
  const vei = document.getElementById('fAluVei').value.toLowerCase();
  const status = document.getElementById('fAluStatus').value;

  const filtrados = listaAlugueis.filter(a =>
    (!cli || (a.clienteNome || '').toLowerCase().includes(cli)) &&
    (!vei || (a.veiculoPlaca || '').toLowerCase().includes(vei) || (a.veiculoModelo || '').toLowerCase().includes(vei)) &&
    (!status || String(a.status).toLowerCase() === status.toLowerCase())
  );

  const tb = document.getElementById('tbAlugueis');
  if (!filtrados.length) {
    tb.innerHTML = '<tr><td colspan="8" class="text-center text-muted">Nenhum registro.</td></tr>';
    return;
  }

  tb.innerHTML = filtrados.map(a => {
    const aberto = String(a.status).toLowerCase() === 'aberto';
    return `
    <tr>
      <td>${a.aluguelId}</td>
      <td>${a.clienteNome || '-'}</td>
      <td>${a.veiculoPlaca || ''} - ${a.veiculoModelo || ''}</td>
      <td>${fmtData(a.dataRetirada)}</td>
      <td>${fmtData(a.dataPrevistaDevolucao)}</td>
      <td>${fmtDinheiro(a.valorTotal)}</td>
      <td>${a.status}</td>
      <td>
        ${aberto ? `
          <button class="btn btn-sm btn-outline-success" onclick="abrirDevolucao(${a.aluguelId}, ${a.quilometragemInicial})">Devolver</button>
          <button class="btn btn-sm btn-outline-primary" onclick="editarAluguel(${a.aluguelId})">Editar</button>
          <button class="btn btn-sm btn-outline-danger" onclick="cancelarAluguel(${a.aluguelId})">Cancelar</button>
        ` : '<span class="text-muted small">Encerrado</span>'}
      </td>
    </tr>`;
  }).join('');
}

function limparFiltroAlu() {
  document.getElementById('fAluCli').value = '';
  document.getElementById('fAluVei').value = '';
  document.getElementById('fAluStatus').value = '';
  renderAlugueis();
}

async function preencherSelectsAluguel() {
  const [clis, veis] = await Promise.all([req('/api/clientes'), req('/api/veiculos')]);
  document.getElementById('aluCliId').innerHTML = '<option value="">Selecione...</option>' +
    clis.map(c => `<option value="${c.clienteId}">${c.nome}</option>`).join('');
  document.getElementById('aluVeiId').innerHTML = '<option value="">Selecione...</option>' +
    veis.map(v => `<option value="${v.veiculoId}">${v.placa} - ${v.modelo} (${v.status})</option>`).join('');
}

async function novoAluguel() {
  await preencherSelectsAluguel();
  const hoje = new Date().toISOString().substring(0, 10);
  document.getElementById('aluId').value = '';
  document.getElementById('aluCliId').disabled = false;
  document.getElementById('aluVeiId').disabled = false;
  document.getElementById('aluRetirada').value = hoje;
  document.getElementById('aluPrevista').value = hoje;
  document.getElementById('aluKm').value = 0;
  document.getElementById('aluDiaria').value = 150;
  document.getElementById('aluObs').value = '';
  document.getElementById('tituloModalAlu').textContent = 'Novo Aluguel';
  mAlu.show();
}

async function editarAluguel(id) {
  try {
    await preencherSelectsAluguel();
    const a = await req('/api/alugueis/' + id);
    document.getElementById('aluId').value = a.aluguelId;
    document.getElementById('aluCliId').value = a.clienteId;
    document.getElementById('aluVeiId').value = a.veiculoId;
    document.getElementById('aluCliId').disabled = true;
    document.getElementById('aluVeiId').disabled = true;
    document.getElementById('aluRetirada').value = a.dataRetirada.substring(0, 10);
    document.getElementById('aluPrevista').value = a.dataPrevistaDevolucao.substring(0, 10);
    document.getElementById('aluKm').value = a.quilometragemInicial;
    document.getElementById('aluDiaria').value = a.valorDiaria;
    document.getElementById('aluObs').value = a.observacoes || '';
    document.getElementById('tituloModalAlu').textContent = 'Editar Aluguel #' + id;
    mAlu.show();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function salvarAluguel(e) {
  e.preventDefault();
  const id = document.getElementById('aluId').value;
  const body = {
    clienteId: Number(document.getElementById('aluCliId').value),
    veiculoId: Number(document.getElementById('aluVeiId').value),
    dataRetirada: document.getElementById('aluRetirada').value,
    dataPrevistaDevolucao: document.getElementById('aluPrevista').value,
    quilometragemInicial: Number(document.getElementById('aluKm').value),
    valorDiaria: Number(document.getElementById('aluDiaria').value),
    observacoes: document.getElementById('aluObs').value.trim() || null
  };
  try {
    if (id) await req('/api/alugueis/' + id, 'PUT', body);
    else await req('/api/alugueis', 'POST', body);
    mAlu.hide();
    mostrarMsg('Aluguel salvo com sucesso!');
    carregarAlugueis();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

function abrirDevolucao(id, kmIni) {
  document.getElementById('devId').value = id;
  document.getElementById('devKm').value = kmIni;
  document.getElementById('devData').value = new Date().toISOString().substring(0, 10);
  mDev.show();
}

async function confirmarDevolucao(e) {
  e.preventDefault();
  const id = document.getElementById('devId').value;
  const body = {
    quilometragemFinal: Number(document.getElementById('devKm').value),
    dataDevolucao: document.getElementById('devData').value || null
  };
  try {
    await req('/api/alugueis/' + id + '/devolucao', 'PATCH', body);
    mDev.hide();
    mostrarMsg('Devolução registrada!');
    carregarAlugueis();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function cancelarAluguel(id) {
  if (!confirm('Cancelar aluguel #' + id + '?')) return;
  try {
    await req('/api/alugueis/' + id, 'DELETE');
    mostrarMsg('Aluguel cancelado!');
    carregarAlugueis();
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

// ==================== CONSULTAS (ETAPA 3) ====================
async function buscarConsulta1() {
  const nome = document.getElementById('c1Nome').value.trim();
  try {
    const dados = await req('/api/filtros/veiculos-por-fabricante?nome=' + encodeURIComponent(nome));
    const tb = document.getElementById('tbC1');
    if (!dados.length) {
      tb.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Nada encontrado.</td></tr>';
      return;
    }
    tb.innerHTML = dados.map(v => `
      <tr><td>${v.veiculoId}</td><td>${v.placa}</td><td>${v.modelo}</td><td>${v.anoFabricacao}</td><td>${v.fabricante}</td><td>${textoStatusVei(v.status)}</td></tr>
    `).join('');
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function buscarConsulta2() {
  const ini = document.getElementById('c2Inicio').value;
  const fim = document.getElementById('c2Fim').value;
  try {
    const dados = await req(`/api/filtros/alugueis-por-periodo?inicio=${ini}&fim=${fim}`);
    const tb = document.getElementById('tbC2');
    if (!dados.length) {
      tb.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Nada encontrado.</td></tr>';
      return;
    }
    tb.innerHTML = dados.map(a => `
      <tr><td>${a.aluguelId}</td><td>${a.cliente}</td><td>${a.placa} - ${a.veiculo}</td><td>${fmtData(a.dataRetirada)}</td><td>${fmtData(a.dataPrevistaDevolucao)}</td><td>${fmtDinheiro(a.valorTotal)}</td></tr>
    `).join('');
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function buscarConsulta3() {
  try {
    const dados = await req('/api/filtros/clientes-com-aluguel-ativo');
    const tb = document.getElementById('tbC3');
    if (!dados.length) {
      tb.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Nenhum cliente com aluguel ativo.</td></tr>';
      return;
    }
    tb.innerHTML = dados.map(c => `
      <tr><td>${c.clienteId}</td><td>${c.nome}</td><td>${c.telefone || '-'}</td><td>${c.aluguelId}</td><td>${fmtData(c.dataRetirada)}</td><td>${fmtData(c.dataPrevistaDevolucao)}</td></tr>
    `).join('');
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function buscarConsulta4() {
  try {
    const dados = await req('/api/filtros/veiculos-sem-aluguel');
    const tb = document.getElementById('tbC4');
    if (!dados.length) {
      tb.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Nenhum veículo sem aluguel.</td></tr>';
      return;
    }
    tb.innerHTML = dados.map(v => `
      <tr><td>${v.veiculoId}</td><td>${v.placa}</td><td>${v.modelo}</td><td>${v.anoFabricacao}</td><td>${textoStatusVei(v.status)}</td></tr>
    `).join('');
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}

async function buscarConsulta5() {
  try {
    const dados = await req('/api/filtros/clientes-com-total-alugueis');
    const tb = document.getElementById('tbC5');
    if (!dados.length) {
      tb.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Nenhum cliente.</td></tr>';
      return;
    }
    tb.innerHTML = dados.map(c => `
      <tr><td>${c.clienteId}</td><td>${c.nome}</td><td>${c.email}</td><td><b>${c.totalAlugueis}</b></td></tr>
    `).join('');
  } catch (e) { mostrarMsg(e.message, 'danger'); }
}
