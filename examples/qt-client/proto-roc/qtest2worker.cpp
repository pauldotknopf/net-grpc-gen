#include "qtest2worker.h"
#include "gen.pb.h"
#include "gen.grpc.pb.h"
#include "roc-lib/qrocobjectadapter.h"
#include "protobuf-qjson/protobufjsonconverter.h"
#include <QThread>
using namespace Tests;
class EventThread : public QThread
{
public:
	std::shared_ptr<Tests::Test2ObjectService::Stub> service;
	grpc::ClientContext context;
	google::protobuf::uint64 objectId;
	QTest2Worker* worker;
	void run() override
	{
		if(!service) { return; }
		Tests::Test2ListenEventStream request;
		request.set_objectid(objectId);
		auto stream = service->ListenEvents(&context, request);
		google::protobuf::Any any;
		while(stream->Read(&any)) { worker->processEvent(&any); }
		auto result = stream->Finish();
		if(result.error_code() == grpc::StatusCode::CANCELLED) { return; }
		if(result.ok()) { qCritical("unabled to stop stream: %s", result.error_message().c_str()); return; }
	}
};
class Tests::QTest2WorkerPrivate
{
public:
	QTest2WorkerPrivate(QTest2Worker* worker) : objectId(0), worker(worker) {}
	QTest2Worker* worker;
	google::protobuf::uint64 objectId;
	std::shared_ptr<Tests::Test2ObjectService::Stub> service;
	grpc::ClientContext objectRequestContext;
	std::unique_ptr<grpc::ClientReader<Tests::Test2CreateResponse>> objectRequest;
	EventThread eventThread;
	void createObject()
	{
		Tests::Test2CreateRequest request;
		objectRequest = service->Create(&objectRequestContext, request);
		Tests::Test2CreateResponse response;
		if(!objectRequest->Read(&response)) { qCritical(""); objectRequest.release(); return; }
		objectId = response.objectid();
		eventThread.objectId = objectId;
		eventThread.service = service;
		eventThread.worker = worker;
		eventThread.start();
	}
	void releaseObject()
	{
		if(objectRequest != nullptr)
		{
			eventThread.context.TryCancel();
			eventThread.quit();
			eventThread.wait();
			objectRequestContext.TryCancel();
			auto result = objectRequest->Finish();
			if(result.error_code() == grpc::StatusCode::CANCELLED) { return; }
			if(!result.ok()) { qCritical("Couldn't release object: %s", result.error_message().c_str()); }
		}
	}
	bool isValid()
	{
		return objectRequest != nullptr;
	}
};
QTest2Worker::QTest2Worker() : QObject(nullptr), d_priv(new QTest2WorkerPrivate(this))
{
	auto channel = QRocObjectAdapter::getSharedChannel();
	if(channel == nullptr) { qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)"); return; }
	d_priv->service = Tests::Test2ObjectService::NewStub(channel);
	d_priv->createObject();
}
QTest2Worker::~QTest2Worker()
{
	d_priv->releaseObject();
}
void QTest2Worker::testMethod2(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test2TestMethod2MethodRequest request;
		Tests::Test2TestMethod2MethodResponse response;
		request.set_objectid(d_priv->objectId);
		if(!val.isNull())
		{
			auto messageVal = new custom::types::TestMessageRequest();
			ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
			request.set_allocated_value(messageVal);
		}
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethod2(&context, request, &response);
		QJsonValue responseValue;
		if(response.has_value())
		{
			auto responseMessageValue = response.value();
			ProtobufJsonConverter::messageToJsonValue(&responseMessageValue, responseValue);
		}
		if(!invokeResult.ok()) {
			emit testMethod2Done(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethod2Done(responseValue, requestId, QString());
		}
	});
}
void QTest2Worker::testMethodSync2(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test2TestMethodSync2MethodRequest request;
		Tests::Test2TestMethodSync2MethodResponse response;
		request.set_objectid(d_priv->objectId);
		if(!val.isNull())
		{
			auto messageVal = new custom::types::TestMessageRequest();
			ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
			request.set_allocated_value(messageVal);
		}
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodSync2(&context, request, &response);
		QJsonValue responseValue;
		if(response.has_value())
		{
			auto responseMessageValue = response.value();
			ProtobufJsonConverter::messageToJsonValue(&responseMessageValue, responseValue);
		}
		if(!invokeResult.ok()) {
			emit testMethodSync2Done(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodSync2Done(responseValue, requestId, QString());
		}
	});
}
void QTest2Worker::testMethodWithNoResponse2(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test2TestMethodWithNoResponse2MethodRequest request;
		Tests::Test2TestMethodWithNoResponse2MethodResponse response;
		request.set_objectid(d_priv->objectId);
		if(!val.isNull())
		{
			auto messageVal = new custom::types::TestMessageRequest();
			ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
			request.set_allocated_value(messageVal);
		}
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodWithNoResponse2(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodWithNoResponse2Done(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodWithNoResponse2Done(requestId, QString());
		}
	});
}
void QTest2Worker::testMethodNoRequest2(int requestId)
{
	QMetaObject::invokeMethod(this, [this, requestId] {
		Tests::Test2TestMethodNoRequest2MethodRequest request;
		Tests::Test2TestMethodNoRequest2MethodResponse response;
		request.set_objectid(d_priv->objectId);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoRequest2(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodNoRequest2Done(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoRequest2Done(requestId, QString());
		}
	});
}
void QTest2Worker::processEvent(void* _event)
{
	auto event = reinterpret_cast<google::protobuf::Any*>(_event);
	if(event->Is<Tests::Test2TestEvent2Event>())
	{
		Tests::Test2TestEvent2Event eventMessage;
		event->UnpackTo(&eventMessage);
		QVariant eventValue = QVariant::fromValue(nullptr);
		if(eventMessage.has_value())
		{
			eventValue = QString::fromStdString(eventMessage.value().value());
		}
		emit testEvent2Raised(eventValue);
	}
	if(event->Is<Tests::Test2TestEventComplex2Event>())
	{
		Tests::Test2TestEventComplex2Event eventMessage;
		event->UnpackTo(&eventMessage);
		QJsonValue eventValue;
		if(eventMessage.has_value())
		{
			auto eventMessageMessageValue = eventMessage.value();
			ProtobufJsonConverter::messageToJsonValue(&eventMessageMessageValue, eventValue);
		}
		emit testEventComplex2Raised(eventValue);
	}
	if(event->Is<Tests::Test2TestEventNoData2Event>())
	{
		emit testEventNoData2Raised();
	}
	if(event->Is<Tests::Test2PropString2PropertyChanged>())
	{
		Tests::Test2PropString2PropertyChanged eventMessage;
		event->UnpackTo(&eventMessage);
		QVariant eventValue = QVariant::fromValue(nullptr);
		if(eventMessage.has_value())
		{
			eventValue = QString::fromStdString(eventMessage.value().value());
		}
		emit propString2Changed(eventValue);
	}
	if(event->Is<Tests::Test2PropComplex2PropertyChanged>())
	{
		Tests::Test2PropComplex2PropertyChanged eventMessage;
		event->UnpackTo(&eventMessage);
		QJsonValue eventValue;
		if(eventMessage.has_value())
		{
			auto eventMessageMessageValue = eventMessage.value();
			ProtobufJsonConverter::messageToJsonValue(&eventMessageMessageValue, eventValue);
		}
		emit propComplex2Changed(eventValue);
	}
	qDebug("got event: %s", event->type_url().c_str());
}
QVariant QTest2Worker::getPropString2()
{
	Tests::Test2PropString2GetRequest request;
	Tests::Test2PropString2GetResponse response;
	request.set_objectid(d_priv->objectId);
	grpc::ClientContext context;
	auto result = d_priv->service->GetPropertyPropString2(&context, request, &response);
	if(!result.ok()) { qCritical("couldn't read property PropString2: %s", result.error_message().c_str()); return QVariant(); }
	QVariant propValue = QVariant::fromValue(nullptr);
	if(response.has_value())
	{
		propValue = QString::fromStdString(response.value().value());
	}
	return propValue;
}
void QTest2Worker::setPropString2(QVariant val)
{
	Tests::Test2PropString2SetRequest request;
	Tests::Test2PropString2SetResponse response;
	request.set_objectid(d_priv->objectId);
	if(val.userType() == QMetaType::QString)
	{
		auto messageVal = new google::protobuf::StringValue();
		messageVal->set_value(val.toString().toStdString());
		request.set_allocated_value(messageVal);
	}
	grpc::ClientContext context;
	auto result = d_priv->service->SetPropertyPropString2(&context, request, &response);
	if(!result.ok()) { qCritical("couldn't set property propString2: %s", result.error_message().c_str()); }
}
QJsonValue QTest2Worker::getPropComplex2()
{
	Tests::Test2PropComplex2GetRequest request;
	Tests::Test2PropComplex2GetResponse response;
	request.set_objectid(d_priv->objectId);
	grpc::ClientContext context;
	auto result = d_priv->service->GetPropertyPropComplex2(&context, request, &response);
	if(!result.ok()) { qCritical("couldn't read property PropComplex2: %s", result.error_message().c_str()); return QJsonValue::Undefined; }
	QJsonValue propValue;
	if(response.has_value())
	{
		auto responseMessageValue = response.value();
		ProtobufJsonConverter::messageToJsonValue(&responseMessageValue, propValue);
	}
	return propValue;
}
void QTest2Worker::setPropComplex2(QJsonValue val)
{
	Tests::Test2PropComplex2SetRequest request;
	Tests::Test2PropComplex2SetResponse response;
	request.set_objectid(d_priv->objectId);
	if(!val.isNull())
	{
		auto messageVal = new custom::types::TestMessageResponse();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
	}
	grpc::ClientContext context;
	auto result = d_priv->service->SetPropertyPropComplex2(&context, request, &response);
	if(!result.ok()) { qCritical("couldn't set property propComplex2: %s", result.error_message().c_str()); }
}
