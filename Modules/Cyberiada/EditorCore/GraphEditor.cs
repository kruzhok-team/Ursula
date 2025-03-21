using System;
using System.Collections.Generic;
using System.Linq;
using Talent.Graphs;
using Action = Talent.Graphs.Action;
using Event = Talent.Graphs.Event;

namespace Talent.GraphEditor.Core
{
    /// <summary>
    /// Редактор графа
    /// </summary>
    public class GraphEditor
    {
        /// <summary>
        /// Документ, хранящий корневой граф
        /// </summary>
        public CyberiadaGraphDocument GraphDocument { get; private set; }

        private IGraphElementViewFactory GraphElementViewFactory { get; }
        private Dictionary<string, Node> _nodes = new Dictionary<string, Node>();
        private Dictionary<string, Edge> _edges = new Dictionary<string, Edge>();

        private BidirectionalDictionary<CyberiadaGraph, IGraphView> _graphViews = new BidirectionalDictionary<CyberiadaGraph, IGraphView>();
        private BidirectionalDictionary<Node, INodeView> _nodeViews = new BidirectionalDictionary<Node, INodeView>();
        private BidirectionalDictionary<Edge, IEdgeView> _edgeViews = new BidirectionalDictionary<Edge, IEdgeView>();
        private BidirectionalDictionary<Event, INodeEventView> _nodeEventViews = new BidirectionalDictionary<Event, INodeEventView>();
        private BidirectionalDictionary<Action, INodeActionView> _nodeActionViews = new BidirectionalDictionary<Action, INodeActionView>();
        private BidirectionalDictionary<Action, IEdgeActionView> _edgeActionViews = new BidirectionalDictionary<Action, IEdgeActionView>();

        private INodeView _initialNodeView;
        private IEdgeView _initialEdgeView;

        /// <summary>
        /// Конструктор редактора графов
        /// </summary>
        /// <param name="factory">Фабрика представлений элементов графа</param>
        public GraphEditor(IGraphElementViewFactory factory)
        {
            GraphElementViewFactory = factory;
        }

        /// <summary>
        /// Устанавливает новый документ и перестраивает представление
        /// </summary>
        /// <param name="graphDocument">Новый документ</param>
        public void SetGraphDocument(CyberiadaGraphDocument graphDocument)
        {
            GraphDocument = graphDocument;
            RebuildView();
        }

        /// <summary>
        /// Пересоздает отсутствующие представления, удаляет ненужные представления.
        /// Полезно, если что-то сломалось или граф изменился в обход редактора
        /// </summary>
        public void RefreshView()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Уничтожает все текущие представления и пересоздает их для текущего графа
        /// </summary>
        public void RebuildView()
        {
            foreach (IEdgeActionView edgeActionView in _edgeActionViews.Values)
            {
                GraphElementViewFactory.DestroyElementView(edgeActionView);
            }

            _edgeActionViews.Clear();

            foreach (INodeActionView nodeActionView in _nodeActionViews.Values)
            {
                GraphElementViewFactory.DestroyElementView(nodeActionView);
            }

            _nodeActionViews.Clear();

            foreach (INodeEventView nodeEventView in _nodeEventViews.Values)
            {
                GraphElementViewFactory.DestroyElementView(nodeEventView);
            }

            _nodeEventViews.Clear();

            foreach (INodeView nodeView in _nodeViews.Values)
            {
                GraphElementViewFactory.DestroyElementView(nodeView);
            }

            _nodeViews.Clear();

            foreach (IEdgeView edgeView in _edgeViews.Values)
            {
                GraphElementViewFactory.DestroyElementView(edgeView);
            }

            _edgeViews.Clear();

            //TODO: придумать как делать DestroyElementView только для граф вью в котором уничтожили все нод вью,
            //сейчас они просто уничтожаются вместе с нодами из-за того что дочерние к нодам в реализации
            _graphViews.Clear();

            _nodes.Clear();
            _edges.Clear();

            _initialNodeView = null;
            _initialEdgeView = null;

            if (GraphDocument?.RootGraph == null)
            {
                return;
            }

            foreach (Node node in GraphDocument.RootGraph.Nodes)
            {
                if (node.Data.Vertex == NodeData.Vertex_Initial)
                {
                    _initialNodeView = CreateViewForNode(node, false);
                }
                else
                {
                    CreateViewForNode(node, false);
                }
            }

            if (_nodes.Count > 0)
            {
                CreateInitialNode();
            }

            foreach (Edge edge in GraphDocument.RootGraph.Edges)
            {
                if (edge.SourceNode == NodeData.Vertex_Initial)
                {
                    _initialEdgeView = CreateViewForEdge(edge);
                }
                else
                {
                    CreateViewForEdge(edge);
                }
            }
        }

        /// <summary>
        /// Подсоединяет представление узла к стартовому узлу
        /// </summary>
        /// <param name="nodeView">Подсоединяемое представление узла</param>
        public void ConnectToInitialNode(INodeView nodeView)
        {
            if (_initialNodeView == null)
            {
                return;
            }

            if (_initialEdgeView != null)
            {
                RemoveEdge(_initialEdgeView);
            }

            _initialEdgeView = CreateNewEdge(_initialNodeView, nodeView, "");
        }

        /// <summary>
        /// Пытается создать копию представления узла
        /// </summary>
        /// <param name="nodeView">Оригинал представления узла</param>
        /// <param name="duplicatedNode">Если копия создалась удачно, возвращает копию, иначе null</param>
        /// <returns>true, если копирование произошло успешно, иначе false</returns>
        public bool TryDuplicateNode(INodeView nodeView, out INodeView duplicatedNode)
        {
            if (!_nodeViews.TryGetValue(nodeView, out Node node))
            {
                duplicatedNode = null;
                return false;
            }

            Node newNode = node.GetCopy(node.Data.GetCopy(), parentNode: node.ParentNode, newID: Guid.NewGuid().ToString());

            if (node.ParentNode != null)
            {
                node.ParentNode.NestedGraph.AddNode(newNode);
            }
            else
            {
                GraphDocument.RootGraph.AddNode(newNode);
            }

            duplicatedNode = CreateViewForNode(newNode, false);
            return true;
        }

        private void CreateInitialNode()
        {
            if (_initialNodeView == null)
            {
                Node nodeData = new(NodeData.Vertex_Initial, new NodeData(NodeData.Vertex_Initial));

                GraphDocument.RootGraph.AddNode(nodeData);
                nodeData.Data.VisualData.Name = "INIT";

                _initialNodeView = CreateViewForNode(nodeData, true);
                _nodeViews.Set(nodeData, _initialNodeView);
            }
        }

        private void CreateRandomInitialEdge()
        {
            if (_initialEdgeView == null && _initialNodeView != null)
            {
                Node targetNode = _nodes.Values.FirstOrDefault(node => node.Data.Vertex != NodeData.Vertex_Initial);

                if (targetNode == null)
                {
                    return;
                }

                Edge edge = new Edge(NodeData.Vertex_Initial + targetNode.ID, NodeData.Vertex_Initial, targetNode.ID, new EdgeData(""));
                GraphDocument.RootGraph.AddEdge(edge);

                if (_nodeViews.TryGetValue(targetNode, out INodeView targetNodeView))
                {
                    _initialEdgeView = GraphElementViewFactory.CreateEdgeView(_initialNodeView, targetNodeView, edge.Data.VisualData, edge.Data.TriggerID, edge.Data.Condition);
                    _initialEdgeView.ID = edge.ID;
                    _edgeViews.Add(edge, _initialEdgeView);
                }
            }
        }

        #region Public Create Methods

        /// <summary>
        /// Создает новое представление графа
        /// </summary>
        /// <param name="id">Уникальный идентификатор графа</param>
        /// <param name="nodeView">Представление графа, содержащее данный граф</param>
        /// <returns>Представление графа</returns>
        public IGraphView CreateNewGraph(string id, INodeView nodeView)
        {
            if (_nodeViews.TryGetValue(nodeView, out Node node) && node.NestedGraph == null)
            {
                node.NestedGraph = new CyberiadaGraph(id, new GraphData());
            }

            IGraphView graphView = GraphElementViewFactory.CreateGraphView(id, nodeView);
            _graphViews.Add(node.NestedGraph, graphView);
            return graphView;
        }

        /// <summary>
        /// Создает новое представление узла
        /// </summary>
        /// <param name="name">Имя узла</param>
        /// <returns>Созданное представление узла</returns>
        public INodeView CreateNewNode(string name)
        {
            Node node = new Node(Guid.NewGuid().ToString(), new NodeData());
            GraphDocument.RootGraph.AddNode(node);
            node.Data.VisualData.Name = name;

            INodeView newNode = CreateViewForNode(node, true);

            if (_initialNodeView == null)
            {
                CreateInitialNode();
                ConnectToInitialNode(newNode);
            }

            return newNode;
        }

        /// <summary>
        /// Создает новое представление ребра
        /// </summary>
        /// <param name="sourceView">Представление входящего узла</param>
        /// <param name="targetView">Представление исходящего узла</param>
        /// <param name="triggerID">Событие перехода</param>
        /// <returns>Созданное представление ребра</returns>
        public IEdgeView CreateNewEdge(INodeView sourceView, INodeView targetView, string triggerID)
        {
            Node sourceNode = _nodeViews.Get(sourceView);
            Node targetNode = _nodeViews.Get(targetView);

            if ((sourceView == _initialNodeView || targetView == _initialNodeView) && _edgeViews.ContainsValue(_initialEdgeView))
            {
                return null;
            }

            Edge edge = new Edge(System.Guid.NewGuid().ToString(), sourceNode.ID, targetNode.ID, new EdgeData(triggerID));
            GraphDocument.RootGraph.AddEdge(edge);

            return CreateViewForEdge(edge);
        }
        
        /// <summary>
        /// Создает предварительное представление ребра
        /// </summary>
        /// <param name="sourceView">Входящее представление узла</param>
        /// <returns>Предварительно созданное представление ребра</returns>
        public IEdgeView CreatePreviewEdgeView(INodeView sourceView)
        {
            IEdgeView edgeView = GraphElementViewFactory.CreatePreviewEdgeView(sourceView);
            return edgeView;
        }

        /// <summary>
        /// Пытается применить настройки предварительного ребра для создания полноценного ребра
        /// </summary>
        /// <param name="edgeView">Предварительно созданное ребро</param>
        /// <param name="sourceView">Входящее представление узла</param>
        /// <param name="targetView">Исходящее представление узла</param>
        /// <param name="triggerID">Идентификатор события перехода</param>
        /// <param name="visualData">Визуальные данные ребра</param>
        /// <returns></returns>
        public bool TryApplyPreview(IEdgeView edgeView, INodeView sourceView, INodeView targetView, string triggerID, EdgeVisualData visualData)
        {
            Node sourceNode = _nodeViews.Get(sourceView);
            Node targetNode = _nodeViews.Get(targetView);

            if (sourceNode == targetNode)
            {
                return false;
            }

            Edge edge = new Edge(System.Guid.NewGuid().ToString(), sourceNode.ID, targetNode.ID, new EdgeData(triggerID));
            edge.Data.VisualData = visualData;
            GraphDocument.RootGraph.AddEdge(edge);
            _edges[edge.ID] = edge;
            edgeView.ID = edge.ID;
            _edgeViews.Add(edge, edgeView);

            foreach (Action action in edge.Data.Actions)
            {
                IEdgeActionView actionView = GraphElementViewFactory.CreateEdgeActionView(edgeView, action.ID);
                _edgeActionViews.Add(action, actionView);
                ChangeEdgeActionParameter(actionView, action.Parameters);
            }

            return true;
        }
        
        /// <summary>
        /// Создает новое представление перехода в узле
        /// </summary>
        /// <param name="nodeView">Представление узла</param>
        /// <param name="triggerID">Событие перехода</param>
        /// <returns>Созданное представление</returns>
        public INodeEventView CreateNewNodeEvent(INodeView nodeView, string triggerID)
        {
            INodeEventView result = default;

            if (_nodeViews.TryGetValue(nodeView, out Node node))
            {
                Event nodeEvent = new Event(triggerID);
                node.Data.AddEvent(nodeEvent);

                if (!_nodeEventViews.ContainsKey(nodeEvent))
                {
                    _nodeEventViews.Set(nodeEvent, GraphElementViewFactory.CreateNodeEventView(_nodeViews.Get(node), triggerID, nodeEvent));
                }

                result = _nodeEventViews.Get(nodeEvent);
            }

            return result;
        }

        /// <summary>
        /// Создает новое представление поведения для перехода в узле
        /// </summary>
        /// <param name="eventView">Представление перехода узла</param>
        /// <param name="actionID">Поведение</param>
        /// <returns>Созданное представление</returns>
        public INodeActionView CreateNewNodeAction(INodeEventView eventView, string actionID)
        {
            INodeActionView result = default;

            if (_nodeEventViews.TryGetValue(eventView, out Event nodeEvent))
            {
                Action eventAction = new Action(actionID);

                nodeEvent.AddAction(eventAction);
                _nodeActionViews.Set(eventAction, GraphElementViewFactory.CreateNodeActionView(eventView, actionID));
                result = _nodeActionViews.Get(eventAction);
            }

            return result;
        }

        /// <summary>
        /// Создает новое представление перехода в ребре
        /// </summary>
        /// <param name="edgeView">Представление ребра</param>
        /// <param name="actionID">Поведение</param>
        /// <returns>Созданное представление</returns>
        public IEdgeActionView CreateNewEdgeAction(IEdgeView edgeView, string actionID)
        {
            IEdgeActionView result = default;

            if (_edgeViews.TryGetValue(edgeView, out Edge edge))
            {
                Action eventAction = new Action(actionID);

                edge.Data.AddAction(eventAction);
                _edgeActionViews.Set(eventAction, GraphElementViewFactory.CreateEdgeActionView(edgeView, actionID));
                result = _edgeActionViews.Get(eventAction);
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Изменяет событие для перехода в узле
        /// </summary>
        /// <param name="nodeEventView">Представление поведения для перехода в узле</param>
        /// <param name="event">Переход узла</param>
        /// <param name="triggerID">Новое событие</param>
        public void ChangeNodeEventTrigger(INodeEventView nodeEventView, Event @event, string triggerID)
        {
            @event.SetTrigger(triggerID);
            nodeEventView.SetTrigger(triggerID);
        }

        /// <summary>
        /// Изменяет событие для перехода в ребре
        /// </summary>
        /// <param name="edgeView">Представление ребра</param>
        /// <param name="triggerID">Новое событие</param>
        public void ChangeEdgeTrigger(IEdgeView edgeView, string triggerID)
        {
            if (_edgeViews.TryGetValue(edgeView, out Edge edge))
            {
                edge.Data.SetTrigger(triggerID);
                edgeView.SetTrigger(triggerID);
            }
        }

        /// <summary>
        /// Изменяет условие для перехода в узле
        /// </summary>
        /// <param name="nodeView">Представление узла</param>
        /// <param name="nodeEventView">Представление перехода в узле</param>
        /// <param name="condition">Новое условие</param>
        public void ChangeNodeEventCondition(INodeView nodeView, INodeEventView nodeEventView, string condition)
        {
            if (_nodeViews.TryGetValue(nodeView, out Node node) && _nodeEventViews.TryGetValue(nodeEventView, out Event nodeEvent) && nodeEvent.Condition != condition)
            {
                nodeEvent.SetCondition(condition);
                nodeEventView.SetCondition(condition);
            }
        }

        /// <summary>
        /// Изменяет условие для перехода в ребре
        /// </summary>
        /// <param name="edgeView">Представление ребра</param>
        /// <param name="condition">Новое условие</param>
        public void ChangeEdgeCondition(IEdgeView edgeView, string condition)
        {
            if (_edgeViews.TryGetValue(edgeView, out Edge edge))
            {
                edge.Data.SetCondition(condition);
                edgeView.SetCondition(condition);
            }
        }

        /// <summary>
        /// Изменяет список параметров поведения для перехода в узле
        /// </summary>
        /// <param name="actionView">Представление поведения</param>
        /// <param name="parameters">Новый список параметров</param>
        public void ChangeNodeActionParameter(INodeActionView actionView, List<Tuple<string, string>> parameters)
        {
            if (_nodeActionViews.TryGetValue(actionView, out Action action))
            {
                action.SetParameters(parameters);
                actionView.SetParameters(parameters);
            }
        }

        /// <summary>
        /// Изменяет список параметров поведения для перехода в ребре
        /// </summary>
        /// <param name="actionView">Представление поведения</param>
        /// <param name="parameters">Новый список параметров</param>
        public void ChangeEdgeActionParameter(IEdgeActionView actionView, List<Tuple<string, string>> parameters)
        {
            if (_edgeActionViews.TryGetValue(actionView, out Action action))
            {
                action.SetParameters(parameters);
                actionView.SetParameters(parameters);
            }
        }

        #region Public Remove Methods

        /// <summary>
        /// Удаляет представление узла
        /// </summary>
        /// <param name="nodeView">Удаляемое представление узла</param>
        /// <param name="createInitialEdge">Необходимо ли, создавать ребро из стартового состояния</param>
        public void RemoveNode(INodeView nodeView, bool createInitialEdge = true)
        {
            if (_nodeViews.TryGetValue(nodeView, out Node node))
            {
                Node oldParent = node.ParentNode;

                foreach (Event nodeEvent in node.Data.Events)
                {
                    foreach (Action action in nodeEvent.Actions)
                    {
                        INodeActionView nodeActionView = _nodeActionViews.Get(action);

                        _nodeActionViews.Remove(nodeActionView);
                        GraphElementViewFactory.DestroyElementView(nodeActionView);
                    }

                    if (_nodeEventViews.TryGetValue(nodeEvent, out INodeEventView eventView))
                    {
                        _nodeEventViews.Remove(nodeEvent);
                        GraphElementViewFactory.DestroyElementView(eventView);
                    }
                }

                foreach (Edge edge in GraphDocument.RootGraph.Edges.Where(edge => edge.SourceNode == node.ID || edge.TargetNode == node.ID).ToArray())
                {
                    if (_edgeViews.TryGetValue(edge, out IEdgeView edgeView))
                    {
                        foreach (Action action in edge.Data.Actions)
                        {
                            IEdgeActionView edgeActionView = _edgeActionViews.Get(action);

                            if (_edgeViews.TryGetValue(edgeView, out Edge nodeEvent) && _edgeActionViews.TryGetValue(edgeActionView, out Action eventAction))
                            {
                                _edgeActionViews.Remove(edgeActionView);
                                GraphElementViewFactory.DestroyElementView(edgeActionView);
                            }
                        }

                        _edges.Remove(edge.ID);
                        _edgeViews.Remove(edgeView);
                        GraphDocument.RootGraph.DeleteEdge(edge);
                        GraphElementViewFactory.DestroyElementView(edgeView);

                        if (edge.SourceNode == NodeData.Vertex_Initial || edge.TargetNode == NodeData.Vertex_Initial)
                        {
                            _initialEdgeView = null;
                        }
                    }
                }

                if (node.NestedGraph != null)
                {
                    foreach (Node child in node.NestedGraph.Nodes.ToArray())
                    {
                        if (_nodeViews.TryGetValue(child, out INodeView childView))
                        {
                            RemoveNode(childView, false);
                        }
                    }
                }

                if (oldParent != null && oldParent.NestedGraph != null)
                {
                    oldParent.NestedGraph.DeleteNode(node);
                }

                _nodes.Remove(node.ID);
                GraphDocument.RootGraph.DeleteNode(node);
                _nodeViews.Remove(node);
                GraphElementViewFactory.DestroyElementView(nodeView);

                if (_nodes.Count > 1)
                {
                    if (createInitialEdge)
                    {
                        CreateRandomInitialEdge();
                    }
                }
                else
                {
                    GraphDocument.RootGraph.DeleteNode(_nodes[NodeData.Vertex_Initial]);
                    GraphElementViewFactory.DestroyElementView(_initialNodeView);
                    _nodeViews.Remove(_initialNodeView);
                    _initialNodeView = null;
                }

                RemoveGraphFromNode(oldParent);
            }
        }

        /// <summary>
        /// Удаляет представление ребра
        /// </summary>
        /// <param name="edgeView">Удаляемое представление ребра</param>
        public void RemoveEdge(IEdgeView edgeView)
        {
            if (_edgeViews.TryGetValue(edgeView, out Edge edge))
            {
                foreach (Action action in edge.Data.Actions)
                {
                    IEdgeActionView edgeActionView = _edgeActionViews.Get(action);

                    _edgeActionViews.Remove(edgeActionView);
                    GraphElementViewFactory.DestroyElementView(edgeActionView);
                }

                _edges.Remove(edge.ID);
                GraphDocument.RootGraph.DeleteEdge(edge);
                _edgeViews.Remove(edgeView);
                GraphElementViewFactory.DestroyElementView(edgeView);
            }
        }

        /// <summary>
        /// Удаляет переход в узле
        /// </summary>
        /// <param name="nodeView">Представление узла, из которого будет удален переход</param>
        /// <param name="nodeEventView">Удаляемое представление перехода</param>
        public void RemoveNodeEvent(INodeView nodeView, INodeEventView nodeEventView)
        {
            Node node = _nodeViews.Get(nodeView);
            Event nodeEvent = _nodeEventViews.Get(nodeEventView);

            foreach (Action action in nodeEvent.Actions)
            {
                if (_nodeActionViews.TryGetValue(action, out INodeActionView nodeActionView))
                {
                    _nodeActionViews.Remove(nodeActionView);
                    GraphElementViewFactory.DestroyElementView(nodeActionView);
                }
            }

            if (_nodeEventViews.TryGetValue(nodeEvent, out INodeEventView eventView))
            {
                _nodeEventViews.Remove(nodeEvent);
                GraphElementViewFactory.DestroyElementView(eventView);
            }

            node.Data.RemoveEvent(nodeEvent);
        }

        /// <summary>
        /// Удаляет представление поведение для перехода в узле
        /// </summary>
        /// <param name="nodeEventView">Представление перехода узла, из которого будет удалено поведение</param>
        /// <param name="nodeActionView">Удаляемое представление поведения</param>
        public void RemoveNodeAction(INodeEventView nodeEventView, INodeActionView nodeActionView)
        {
            if (_nodeEventViews.TryGetValue(nodeEventView, out Event nodeEvent) && _nodeActionViews.TryGetValue(nodeActionView, out Action eventAction))
            {
                nodeEvent.RemoveAction(eventAction);
                _nodeActionViews.Remove(nodeActionView);
                GraphElementViewFactory.DestroyElementView(nodeActionView);
            }
        }

        /// <summary>
        /// Удаляет представление поведение для перехода в ребре
        /// </summary>
        /// <param name="edgeView">Представление перехода ребра, из которого будет удалено поведение</param>
        /// <param name="edgeActionView">Удаляемое представление поведения</param>
        public void RemoveEdgeAction(IEdgeView edgeView, IEdgeActionView edgeActionView)
        {
            if (_edgeViews.TryGetValue(edgeView, out Edge nodeEvent) && _edgeActionViews.TryGetValue(edgeActionView, out Action eventAction))
            {
                nodeEvent.Data.RemoveAction(eventAction);
                _edgeActionViews.Remove(edgeActionView);
                GraphElementViewFactory.DestroyElementView(edgeActionView);
            }
        }

        #endregion

        /// <summary>
        /// Устанавливает нового родителя для представления узла
        /// </summary>
        /// <param name="childView">Представление дочернего узла</param>
        /// <param name="parentView">Представление родительского узла</param>
        /// <param name="layoutAutomatically"></param>
        public void SetParent(INodeView childView, INodeView parentView, bool layoutAutomatically)
        {
            if (childView == null || !_nodeViews.TryGetValue(childView, out Node child) || _initialNodeView != null && childView == _initialNodeView)
            {
                return;
            }

            Node oldParent = null;

            if (parentView == null)
            {
                if (child.ParentNode != null)
                {
                    child.ParentNode.NestedGraph.DeleteNode(child);
                    GraphDocument.RootGraph.AddNode(child);
                }

                oldParent = child.ParentNode;
                child.ParentNode = null;

                _nodeViews.Get(child).SetParent(null, layoutAutomatically);
            }
            else if (_nodeViews.TryGetValue(parentView, out Node parent) && child != parent && (_initialNodeView == null || parentView != _initialNodeView))
            {
                if (child.ParentNode != parent)
                {
                    if (NodeHasNodeRecursively(child, parent))
                    {
                        return;
                    }

                    //удаление чайлд ноды из корневого графа или из графа текущего родителя
                    if (GraphDocument.RootGraph.TryGetNode(child.ID, out _))
                    {
                        GraphDocument.RootGraph.DeleteNode(child);
                    }
                    else
                    {
                        child.ParentNode?.NestedGraph.DeleteNode(child);
                    }

                    //смена родителя
                    oldParent = child.ParentNode;
                    child.ParentNode = parent;

                    //создание вложенного графа родителю, если его нет
                    if (parent.NestedGraph == null)
                    {
                        CreateNewGraph(System.Guid.NewGuid().ToString(), parentView);
                    }

                    //добавление чайлд ноды в вложенный граф нового родителя
                    parent.NestedGraph.AddNode(child);
                }

                _nodeViews.Get(child).SetParent(_graphViews.Get(parent.NestedGraph), layoutAutomatically);
            }

            RemoveGraphFromNode(oldParent);
        }
        
        /// <summary>
        /// Пытается найти представление узла с определенным уникальным идентификатором
        /// </summary>
        /// <param name="id">Уникальный идентификатор представления узла</param>
        /// <param name="nodeView">Возвращает представление узла, если представление с таким соответствующим идентификатором есть в графе, иначе null</param>
        /// <returns>true если представление узла найдено, иначе false</returns>
        public bool TryGetNodeViewByID(string id, out INodeView nodeView)
        {
            if (!_nodes.TryGetValue(id, out Node node))
            {
                nodeView = null;
                return false;
            }
            
            return _nodeViews.TryGetValue(node, out nodeView);
        }

        /// <summary>
        /// Пытается найти представление ребра с определенным уникальным идентификатором
        /// </summary>
        /// <param name="id">Уникальный идентификатор представления ребра</param>
        /// <param name="edgeView">Возвращает представление ребра, если представление с таким соответствующим идентификатором есть в графе, иначе null</param>
        /// <returns>true если представление ребра найдено, иначе false</returns>
        public bool TryGetEdgeViewByID(string id, out IEdgeView edgeView)
        {
            if (!_edges.TryGetValue(id, out Edge edge))
            {
                edgeView = null;
                return false;
            }
            
            return _edgeViews.TryGetValue(edge, out edgeView);
        }

        // TODO: remove recursion
        private bool NodeHasNodeRecursively(Node node1, Node node2)
        {
            if (node1 == null || node1.NestedGraph == null || node2 == null)
            {
                return false;
            }

            if (node1.NestedGraph.HasNode(node2.ID))
            {
                return true;
            }

            foreach (Node nestedNode in node1.NestedGraph.Nodes)
            {
                if (NodeHasNodeRecursively(nestedNode, node2))
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveGraphFromNode(Node node)
        {
            if (node == null || node.NestedGraph == null)
            {
                return;
            }

            if (node.NestedGraph.Nodes.Count == 0)
            {
                if (_graphViews.TryGetValue(node.NestedGraph, out IGraphView graphView))
                {
                    _graphViews.Remove(graphView);
                    GraphElementViewFactory.DestroyElementView(graphView);
                }

                node.NestedGraph = null;
            }
        }

        #region Internal Create Methods

        private INodeView CreateViewForNode(Node node, bool layoutAutomatically)
        {
            _nodes[node.ID] = node;
            INodeView view = GraphElementViewFactory.CreateNodeView(node.Data.VisualData, node.Data.Vertex, layoutAutomatically);
            view.ID = node.ID;
            _nodeViews.Set(node, view);

            foreach (Event nodeEvent in node.Data.Events)
            {
                string[] actionsArray = new string[nodeEvent.Actions.Count];

                for (int i = 0; i < actionsArray.Length; i++)
                {
                    actionsArray[i] = nodeEvent.Actions[i].ID;
                }

                INodeEventView eventView = GraphElementViewFactory.CreateNodeEventView(view, nodeEvent.TriggerID, nodeEvent);
                _nodeEventViews.Add(nodeEvent, eventView);
                eventView.SetCondition(nodeEvent.Condition);

                foreach (Action action in nodeEvent.Actions)
                {
                    var actionView = GraphElementViewFactory.CreateNodeActionView(eventView, action.ID);
                    _nodeActionViews.Add(action, actionView);
                    ChangeNodeActionParameter(actionView, action.Parameters);
                }
            }

            if (node.NestedGraph != null)
            {
                CreateNewGraph(node.NestedGraph.ID, view);

                foreach (Node nestedNode in node.NestedGraph.Nodes)
                {
                    INodeView nestedView = CreateViewForNode(nestedNode, layoutAutomatically);
                    SetParent(nestedView, view, layoutAutomatically);
                }
            }

            return view;
        }

        private IEdgeView CreateViewForEdge(Edge edge)
        {
            IEdgeView edgeView = null;

            if (_nodeViews.TryGetValue(_nodes[edge.SourceNode], out INodeView sourceNodeView) &&
                _nodeViews.TryGetValue(_nodes[edge.TargetNode], out INodeView targetNodeView))
            {
                _edges[edge.ID] = edge;
                edgeView = GraphElementViewFactory.CreateEdgeView(sourceNodeView, targetNodeView, edge.Data.VisualData, edge.Data.TriggerID, edge.Data.Condition);
                edgeView.ID = edge.ID;
                _edgeViews.Add(edge, edgeView);

                foreach (Action action in edge.Data.Actions)
                {
                    var actionView = GraphElementViewFactory.CreateEdgeActionView(edgeView, action.ID);
                    _edgeActionViews.Add(action, actionView);
                    ChangeEdgeActionParameter(actionView, action.Parameters);
                }
            }

            return edgeView;
        }

        #endregion
    }

    #region Interfaces

    /// <summary>
    /// Фабрика представлений элементов графа
    /// </summary>
    public interface IGraphElementViewFactory
    {
        /// <summary>
        /// Создает новое представление графа
        /// </summary>
        /// <param name="graphID">Уникальный идентификатор графа</param>
        /// <param name="parentNodeView">Представление родительской вершины</param>
        /// <returns>Представление графа</returns>
        IGraphView CreateGraphView(string graphID, INodeView parentNodeView);

        /// <summary>
        /// Создает новое представление узла
        /// </summary>
        /// <param name="visualData">Данные для визуального представления узла</param>
        /// <param name="vertex">Имя вершины</param>
        /// <param name="layoutAutomatically"></param>
        /// <returns>Представление узла</returns>
        INodeView CreateNodeView(NodeVisualData visualData, string vertex, bool layoutAutomatically);

        /// <summary>
        /// Создает представление перехода в узле
        /// </summary>
        /// <param name="nodeView">Представление узла</param>
        /// <param name="triggerID">Событие</param>
        /// <param name="event">Переход</param>
        /// <returns>Представление перехода</returns>
        INodeEventView CreateNodeEventView(INodeView nodeView, string triggerID, Event @event);

        /// <summary>
        /// Создает представление поведения в переходе для узла 
        /// </summary>
        /// <param name="eventView">Представление перехода</param>
        /// <param name="actionID">Переход</param>
        /// <returns>Представление поведения</returns>
        INodeActionView CreateNodeActionView(INodeEventView eventView, string actionID);

        /// <summary>
        /// Создает представление ребра
        /// </summary>
        /// <param name="sourceNode">Входящее представление узла</param>
        /// <param name="targetNode">Исходящее представление узла</param>
        /// <param name="edgeVisualData">Данные для представления ребра</param>
        /// <param name="triggerID">Событие</param>
        /// <param name="condition">Условие</param>
        /// <returns>Представление ребра</returns>
        IEdgeView CreateEdgeView(INodeView sourceNode, INodeView targetNode, EdgeVisualData edgeVisualData,
            string triggerID, string condition);
        
        /// <summary>
        /// Создает предварительное представление ребра
        /// </summary>
        /// <param name="sourceView">Входящее представление узла</param>
        /// <returns>Предварительно созданное представление ребра</returns>
        IEdgeView CreatePreviewEdgeView(INodeView sourceView);

        /// <summary>
        /// Создает представление поведения в переходе для ребра
        /// </summary>
        /// <param name="edgeView">Представление ребра</param>
        /// <param name="actionID">Поведение</param>
        /// <returns>Представление поведения</returns>
        IEdgeActionView CreateEdgeActionView(IEdgeView edgeView, string actionID);

        /// <summary>
        /// Разрушает представление элемента графа 
        /// </summary>
        /// <param name="view">Элемент графа</param>
        void DestroyElementView(IGraphElementView view);
    }

    /// <summary>
    /// Представление графа
    /// </summary>
    public interface IGraphView : IGraphElementView { }

    /// <summary>
    /// Представление элемента графа
    /// </summary>
    public interface IGraphElementView { }

    /// <summary>
    /// Представление узла
    /// </summary>
    public interface INodeView : IGraphElementView
    {
        /// <summary>
        /// Устанавливает родительский граф для узла
        /// </summary>
        /// <param name="parent">Родительский граф</param>
        /// <param name="layoutAutomatically"></param>
        void SetParent(IGraphView parent, bool layoutAutomatically);
        
        /// <summary>
        /// Уникальный идентификатор представления узла
        /// </summary>
        string ID { get; set; }
    }

    /// <summary>
    /// Представление перехода в узле
    /// </summary>
    public interface INodeEventView : IGraphElementView
    {
        /// <summary>
        /// Устанавливает условие для перехода
        /// </summary>
        /// <param name="condition">Условие</param>
        void SetCondition(string condition);

        /// <summary>
        /// Устанавливает событие для перехода
        /// </summary>
        /// <param name="triggerID">Событие</param>
        void SetTrigger(string triggerID);
    }

    /// <summary>
    /// Представление поведения для перехода в узле
    /// </summary>
    public interface INodeActionView : IGraphElementView
    {
        /// <summary>
        /// Устанавливает список параметров для поведения
        /// </summary>
        /// <param name="parameters">Список параметров</param>
        void SetParameters(List<Tuple<string, string>> parameters);
    }

    /// <summary>
    /// Представление ребра
    /// </summary>
    public interface IEdgeView : IGraphElementView
    {
        /// <summary>
        /// Устанавливает событие для перехода
        /// </summary>
        /// <param name="triggerID">Событие</param>
        void SetTrigger(string triggerID);

        /// <summary>
        /// Устанавливает условие для перехода
        /// </summary>
        /// <param name="condition">Условие</param>
        void SetCondition(string condition);
        
        /// <summary>
        /// Уникальный идентификатор представления ребра
        /// </summary>
        string ID { get; set; }
    }

    /// <summary>
    /// Представление поведения для перехода в ребре
    /// </summary>
    public interface IEdgeActionView : IGraphElementView
    {
        /// <summary>
        /// Устанавливает список параметров для поведения
        /// </summary>
        /// <param name="parameters">Список параметров</param>
        void SetParameters(List<Tuple<string, string>> parameters);
    }

    #endregion
}